namespace Lootopia.Api.Features.Commissions.GetCommissionSchemas;

public record GetCommissionSchemasResponse(IReadOnlyList<CommissionSchemaDto> Schemas);

public record CommissionSchemaDto(
    Guid Id,
    string Type,
    decimal Value,
    decimal PlatformShare,
    decimal OrganiserShare,
    bool IsDefault,
    DateTime CreatedAt);
