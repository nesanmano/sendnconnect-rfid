using System.Linq.Expressions;
using System.Reflection;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.Entities;
using CardManagement.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Data;

public class CardManagementDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public CardManagementDbContext(DbContextOptions<CardManagementDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CardHolder> CardHolders => Set<CardHolder>();
    public DbSet<CardType> CardTypes => Set<CardType>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BlacklistedUid> BlacklistedUids => Set<BlacklistedUid>();
    public DbSet<BatchOperation> BatchOperations => Set<BatchOperation>();
    public DbSet<AutoTopupRule> AutoTopupRules => Set<AutoTopupRule>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<CustomFieldIndex> CustomFieldIndexes => Set<CustomFieldIndex>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Apply global tenant query filter to all ITenantEntity types ──
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var orgIdProperty = Expression.Property(parameter, nameof(ITenantEntity.OrgId));

            // Access _tenantContext.OrgId via closure
            var tenantCtx = Expression.Constant(this);
            var tenantField = Expression.Field(tenantCtx, typeof(CardManagementDbContext).GetField("_tenantContext", BindingFlags.NonPublic | BindingFlags.Instance)!);
            var tenantOrgId = Expression.Property(tenantField, nameof(ITenantContext.OrgId));

            var filter = Expression.Lambda(Expression.Equal(orgIdProperty, tenantOrgId), parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // ── TenantSettings ──
        modelBuilder.Entity<TenantSettings>(e =>
        {
            e.ToTable("tenant_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256);
            e.Property(x => x.DefaultCurrency).HasColumnName("default_currency").HasMaxLength(8);
            e.Property(x => x.Timezone).HasColumnName("timezone").HasMaxLength(64);
            e.Property(x => x.MaxCardsPerHolder).HasColumnName("max_cards_per_holder");
            e.Property(x => x.AllowNegativeBalance).HasColumnName("allow_negative_balance");
            e.Property(x => x.SettingsJson).HasColumnName("settings_json").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.OrgId).IsUnique().HasDatabaseName("uq_tenant_settings_org");
        });

        // ── CustomFieldDefinition ──
        modelBuilder.Entity<CustomFieldDefinition>(e =>
        {
            e.ToTable("custom_field_definitions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.EntityType).HasColumnName("entity_type").HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.FieldKey).HasColumnName("field_key").HasMaxLength(128);
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256);
            e.Property(x => x.FieldType).HasColumnName("field_type").HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.IsRequired).HasColumnName("is_required");
            e.Property(x => x.IsSearchable).HasColumnName("is_searchable");
            e.Property(x => x.IsPii).HasColumnName("is_pii");
            e.Property(x => x.DisplayOrder).HasColumnName("display_order");
            e.Property(x => x.OptionsJson).HasColumnName("options_json").HasColumnType("json");
            e.Property(x => x.DefaultValue).HasColumnName("default_value").HasMaxLength(512);
            e.Property(x => x.ValidationRegex).HasColumnName("validation_regex").HasMaxLength(512);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.EntityType, x.FieldKey }).IsUnique().HasDatabaseName("uq_cfd_org_entity_key");
        });

        // ── CardHolder ──
        modelBuilder.Entity<CardHolder>(e =>
        {
            e.ToTable("card_holders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(256);
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256);
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CustomData).HasColumnName("custom_data").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.ExternalId }).IsUnique().HasDatabaseName("uq_ch_org_ext");
        });

        // ── CardType ──
        modelBuilder.Entity<CardType>(e =>
        {
            e.ToTable("card_types");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(128);
            e.Property(x => x.Description).HasColumnName("description").HasMaxLength(512);
            e.Property(x => x.DefaultCurrency).HasColumnName("default_currency").HasMaxLength(8);
            e.Property(x => x.DailySpendLimit).HasColumnName("daily_spend_limit").HasColumnType("decimal(18,4)");
            e.Property(x => x.MonthlySpendLimit).HasColumnName("monthly_spend_limit").HasColumnType("decimal(18,4)");
            e.Property(x => x.MaxBalance).HasColumnName("max_balance").HasColumnType("decimal(18,4)");
            e.Property(x => x.ValidityDays).HasColumnName("validity_days");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CustomData).HasColumnName("custom_data").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.Name }).IsUnique().HasDatabaseName("uq_ct_org_name");
        });

        // ── Card ──
        modelBuilder.Entity<Card>(e =>
        {
            e.ToTable("cards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.HolderId).HasColumnName("holder_id").HasColumnType("char(36)");
            e.Property(x => x.CardTypeId).HasColumnName("card_type_id").HasColumnType("char(36)");
            e.Property(x => x.Uid).HasColumnName("uid").HasMaxLength(128);
            e.Property(x => x.Label).HasColumnName("label").HasMaxLength(256);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.BlockReason).HasColumnName("block_reason").HasMaxLength(512);
            e.Property(x => x.ReplacedById).HasColumnName("replaced_by_id").HasColumnType("char(36)");
            e.Property(x => x.CustomData).HasColumnName("custom_data").HasColumnType("json");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.RegisteredAt).HasColumnName("registered_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.Uid }).IsUnique().HasDatabaseName("uq_cards_org_uid");
            e.HasOne(x => x.Holder).WithMany(h => h.Cards).HasForeignKey(x => x.HolderId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.CardType).WithMany(ct => ct.Cards).HasForeignKey(x => x.CardTypeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ReplacedBy).WithOne().HasForeignKey<Card>(x => x.ReplacedById).OnDelete(DeleteBehavior.SetNull);
        });

        // ── Wallet ──
        modelBuilder.Entity<Wallet>(e =>
        {
            e.ToTable("wallets");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.CardId).HasColumnName("card_id").HasColumnType("char(36)");
            e.Property(x => x.Balance).HasColumnName("balance").HasColumnType("decimal(18,4)");
            e.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(8);
            e.Property(x => x.DailySpent).HasColumnName("daily_spent").HasColumnType("decimal(18,4)");
            e.Property(x => x.DailyResetAt).HasColumnName("daily_reset_at");
            e.Property(x => x.MonthlySpent).HasColumnName("monthly_spent").HasColumnType("decimal(18,4)");
            e.Property(x => x.MonthlyResetAt).HasColumnName("monthly_reset_at");
            e.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.CardId }).IsUnique().HasDatabaseName("uq_wallets_org_card");
            e.HasOne(x => x.Card).WithOne(c => c.Wallet).HasForeignKey<Wallet>(x => x.CardId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Merchant ──
        modelBuilder.Entity<Merchant>(e =>
        {
            e.ToTable("merchants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            e.Property(x => x.Location).HasColumnName("location").HasMaxLength(512);
            e.Property(x => x.TerminalId).HasColumnName("terminal_id").HasMaxLength(128);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CustomData).HasColumnName("custom_data").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.TerminalId }).IsUnique().HasDatabaseName("uq_merchants_org_tid");
        });

        // ── Transaction ──
        modelBuilder.Entity<Transaction>(e =>
        {
            e.ToTable("transactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.WalletId).HasColumnName("wallet_id").HasColumnType("char(36)");
            e.Property(x => x.MerchantId).HasColumnName("merchant_id").HasColumnType("char(36)");
            e.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,4)");
            e.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("decimal(18,4)");
            e.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(256);
            e.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(128);
            e.Property(x => x.RelatedTxnId).HasColumnName("related_txn_id").HasColumnType("char(36)");
            e.Property(x => x.Description).HasColumnName("description").HasMaxLength(512);
            e.Property(x => x.CustomData).HasColumnName("custom_data").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.OrgId, x.IdempotencyKey }).IsUnique().HasDatabaseName("uq_txn_org_idem");
            e.HasOne(x => x.Wallet).WithMany(w => w.Transactions).HasForeignKey(x => x.WalletId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Merchant).WithMany(m => m.Transactions).HasForeignKey(x => x.MerchantId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RelatedTxn).WithOne().HasForeignKey<Transaction>(x => x.RelatedTxnId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── AuditLog ──
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(64);
            e.Property(x => x.EntityId).HasColumnName("entity_id").HasColumnType("char(36)");
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(64);
            e.Property(x => x.ActorId).HasColumnName("actor_id").HasMaxLength(128);
            e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            e.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
            e.Property(x => x.ChangesJson).HasColumnName("changes_json").HasColumnType("json");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.OrgId, x.EntityType, x.EntityId }).HasDatabaseName("ix_al_org_entity");
        });

        // ── BlacklistedUid ──
        modelBuilder.Entity<BlacklistedUid>(e =>
        {
            e.ToTable("blacklisted_uids");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.Uid).HasColumnName("uid").HasMaxLength(128);
            e.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(512);
            e.Property(x => x.BlockedBy).HasColumnName("blocked_by").HasMaxLength(128);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => new { x.OrgId, x.Uid }).IsUnique().HasDatabaseName("uq_bl_org_uid");
        });

        // ── BatchOperation ──
        modelBuilder.Entity<BatchOperation>(e =>
        {
            e.ToTable("batch_operations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.TotalCount).HasColumnName("total_count");
            e.Property(x => x.SuccessCount).HasColumnName("success_count");
            e.Property(x => x.FailureCount).HasColumnName("failure_count");
            e.Property(x => x.ErrorDetails).HasColumnName("error_details").HasColumnType("json");
            e.Property(x => x.InitiatedBy).HasColumnName("initiated_by").HasMaxLength(128);
            e.Property(x => x.StartedAt).HasColumnName("started_at");
            e.Property(x => x.CompletedAt).HasColumnName("completed_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // ── AutoTopupRule ──
        modelBuilder.Entity<AutoTopupRule>(e =>
        {
            e.ToTable("auto_topup_rules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.WalletId).HasColumnName("wallet_id").HasColumnType("char(36)");
            e.Property(x => x.Threshold).HasColumnName("threshold").HasColumnType("decimal(18,4)");
            e.Property(x => x.TopupAmount).HasColumnName("topup_amount").HasColumnType("decimal(18,4)");
            e.Property(x => x.MaxDailyTopups).HasColumnName("max_daily_topups");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.LastTriggered).HasColumnName("last_triggered");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.OrgId, x.WalletId }).IsUnique().HasDatabaseName("uq_atr_org_wallet");
            e.HasOne(x => x.Wallet).WithOne(w => w.AutoTopupRule).HasForeignKey<AutoTopupRule>(x => x.WalletId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── WebhookSubscription ──
        modelBuilder.Entity<WebhookSubscription>(e =>
        {
            e.ToTable("webhook_subscriptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(128);
            e.Property(x => x.TargetUrl).HasColumnName("target_url").HasMaxLength(1024);
            e.Property(x => x.Secret).HasColumnName("secret").HasMaxLength(256);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.LastTriggered).HasColumnName("last_triggered");
            e.Property(x => x.FailureCount).HasColumnName("failure_count");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // ── NotificationLog ──
        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.ToTable("notification_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.HolderId).HasColumnName("holder_id").HasColumnType("char(36)");
            e.Property(x => x.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(128);
            e.Property(x => x.Recipient).HasColumnName("recipient").HasMaxLength(256);
            e.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(256);
            e.Property(x => x.BodyPreview).HasColumnName("body_preview").HasMaxLength(1024);
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1024);
            e.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(256);
            e.Property(x => x.SentAt).HasColumnName("sent_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Holder).WithMany().HasForeignKey(x => x.HolderId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── CustomFieldIndex ──
        modelBuilder.Entity<CustomFieldIndex>(e =>
        {
            e.ToTable("custom_field_index");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasColumnType("char(36)");
            e.Property(x => x.OrgId).HasColumnName("org_id").HasMaxLength(128).IsRequired();
            e.Property(x => x.EntityType).HasColumnName("entity_type").HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.EntityId).HasColumnName("entity_id").HasColumnType("char(36)");
            e.Property(x => x.FieldKey).HasColumnName("field_key").HasMaxLength(128);
            e.Property(x => x.FieldValue).HasColumnName("field_value").HasMaxLength(512);
            e.HasIndex(x => new { x.OrgId, x.EntityType, x.FieldKey, x.FieldValue }).HasDatabaseName("ix_cfi_lookup");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (string.IsNullOrEmpty(entry.Entity.OrgId) && _tenantContext.IsResolved)
                    entry.Entity.OrgId = _tenantContext.OrgId;
            }

            // Auto-set UpdatedAt for modified entities
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProp is not null)
                    updatedAtProp.CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
