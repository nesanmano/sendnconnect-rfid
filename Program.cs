using System.Security.Claims;
using System.Text.Json.Serialization;
using Azure.Identity;
using CardManagement.Api.Configuration;
using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Azure App Configuration ────────────────────────────────
var azureAppConfigEndpoint = builder.Configuration["AZURE_APPCONFIG_ENDPOINT"];

if (!string.IsNullOrEmpty(azureAppConfigEndpoint))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(azureAppConfigEndpoint), new DefaultAzureCredential())
               .ConfigureKeyVault(kv =>
               {
                   kv.SetCredential(new DefaultAzureCredential());
               })
               .ConfigureRefresh(refresh =>
               {
                   refresh.Register("Sentinel", refreshAll: true)
                          .SetCacheExpiration(TimeSpan.FromMinutes(5));
               });
    });
}

// ── Configuration bindings ─────────────────────────────────
var zitadelSettings = builder.Configuration.GetSection(ZitadelSettings.SectionName).Get<ZitadelSettings>() ?? new ZitadelSettings();
builder.Services.Configure<ZitadelSettings>(builder.Configuration.GetSection(ZitadelSettings.SectionName));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName));

// ── Database (MySQL via Pomelo) ────────────────────────────
// Connection string comes from App Config; username/password from Key Vault references
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it in Azure App Configuration or appsettings.json.");

// Append Key Vault-sourced credentials (reusing existing Spring vault secrets)
var dbUser = builder.Configuration["spring-datasource-username"];
var dbPass = builder.Configuration["spring-datasource-password"];

if (!string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPass))
{
    connectionString = $"{connectionString.TrimEnd(';')};User={dbUser};Password={dbPass};";
}

builder.Services.AddDbContext<CardManagementDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysql =>
    {
        mysql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        mysql.CommandTimeout(30);
    });
});

// ── Tenant context (scoped per request) ────────────────────
builder.Services.AddScoped<ITenantContext, TenantContext>();

// ── Services ───────────────────────────────────────────────
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICardHolderService, CardHolderService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
builder.Services.AddScoped<ITenantSettingsService, TenantSettingsService>();

// ── Kafka producer (singleton) ─────────────────────────────
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// ── Authentication (Zitadel JWT) ───────────────────────────
var bypassAuth = builder.Configuration.GetValue<bool>("BypassAuth");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = zitadelSettings.Authority;
        options.Audience = zitadelSettings.Audience;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !bypassAuth,
            ValidateAudience = !bypassAuth,
            ValidateLifetime = !bypassAuth,
            ValidIssuer = zitadelSettings.Authority,
            ValidAudience = zitadelSettings.Audience,
            RoleClaimType = "urn:zitadel:iam:org:project:roles",
            NameClaimType = "sub"
        };

        if (bypassAuth)
        {
            options.RequireHttpsMetadata = false;
            // In dev mode we never validate tokens — the bypass middleware injects claims
            options.TokenValidationParameters.SignatureValidator = (token, _) => new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token);
        }
    });

builder.Services.AddAuthorization();

// ── Controllers ────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SendNConnect RFID API",
        Version = "v1",
        Description = "SendNConnect — Multi-tenant RFID/NFC card lifecycle, wallet, and transaction management platform."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Health checks ──────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CardManagementDbContext>("mysql");

// ── CORS ───────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["*"];
        if (origins.Contains("*"))
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SendNConnect RFID API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ── Dev auth bypass middleware ──────────────────────────────
if (bypassAuth)
{
    app.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-001"),
                new Claim("sub", "dev-user-001"),
                new Claim("org_id", "dev-org-001"),
                new Claim("urn:zitadel:iam:org:id", "dev-org-001"),
                new Claim("email", "dev@localhost"),
                new Claim("name", "Dev User")
            };
            var identity = new ClaimsIdentity(claims, "DevBypass");
            context.User = new ClaimsPrincipal(identity);
        }
        await next();
    });

    app.Logger.LogWarning("⚠️  BypassAuth is ENABLED — all requests use dev-org-001 / dev-user-001. DO NOT use in production!");
}

app.UseMiddleware<TenantMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
