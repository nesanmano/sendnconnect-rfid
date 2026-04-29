using CardManagement.Api.Models.DTOs.CustomFields;
using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Services;

public interface ICustomFieldService
{
    Task<CustomFieldResponse> CreateAsync(CreateCustomFieldRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CustomFieldResponse>> ListAsync(CustomFieldEntityType entityType, CancellationToken ct = default);
    Task<CustomFieldResponse> UpdateAsync(string id, CreateCustomFieldRequest request, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
