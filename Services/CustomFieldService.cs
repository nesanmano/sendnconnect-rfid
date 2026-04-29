using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.CustomFields;
using CardManagement.Api.Models.Entities;
using CardManagement.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class CustomFieldService : ICustomFieldService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;

    public CustomFieldService(CardManagementDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CustomFieldResponse> CreateAsync(CreateCustomFieldRequest request, CancellationToken ct = default)
    {
        var exists = await _db.CustomFieldDefinitions
            .AnyAsync(f => f.EntityType == request.EntityType && f.FieldKey == request.FieldKey, ct);
        if (exists)
            throw new InvalidOperationException($"Field '{request.FieldKey}' already defined for {request.EntityType}.");

        var field = new CustomFieldDefinition
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            EntityType = request.EntityType,
            FieldKey = request.FieldKey,
            DisplayName = request.DisplayName ?? request.FieldKey,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            IsSearchable = request.IsSearchable,
            IsPii = request.IsPii,
            DisplayOrder = request.DisplayOrder,
            OptionsJson = request.OptionsJson,
            DefaultValue = request.DefaultValue,
            ValidationRegex = request.ValidationRegex,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.CustomFieldDefinitions.Add(field);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(field);
    }

    public async Task<IReadOnlyList<CustomFieldResponse>> ListAsync(CustomFieldEntityType entityType, CancellationToken ct = default)
    {
        var fields = await _db.CustomFieldDefinitions
            .Where(f => f.EntityType == entityType && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.FieldKey)
            .ToListAsync(ct);

        return fields.Select(MapToResponse).ToList();
    }

    public async Task<CustomFieldResponse> UpdateAsync(string id, CreateCustomFieldRequest request, CancellationToken ct = default)
    {
        var field = await _db.CustomFieldDefinitions.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Custom field '{id}' not found.");

        field.DisplayName = request.DisplayName ?? request.FieldKey;
        field.FieldType = request.FieldType;
        field.IsRequired = request.IsRequired;
        field.IsSearchable = request.IsSearchable;
        field.IsPii = request.IsPii;
        field.DisplayOrder = request.DisplayOrder;
        field.OptionsJson = request.OptionsJson;
        field.DefaultValue = request.DefaultValue;
        field.ValidationRegex = request.ValidationRegex;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(field);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var field = await _db.CustomFieldDefinitions.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Custom field '{id}' not found.");

        field.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    private static CustomFieldResponse MapToResponse(CustomFieldDefinition f) => new()
    {
        Id = f.Id,
        EntityType = f.EntityType,
        FieldKey = f.FieldKey,
        DisplayName = f.DisplayName,
        FieldType = f.FieldType,
        IsRequired = f.IsRequired,
        IsSearchable = f.IsSearchable,
        IsPii = f.IsPii,
        DisplayOrder = f.DisplayOrder,
        OptionsJson = f.OptionsJson,
        DefaultValue = f.DefaultValue,
        ValidationRegex = f.ValidationRegex,
        IsActive = f.IsActive,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt
    };
}
