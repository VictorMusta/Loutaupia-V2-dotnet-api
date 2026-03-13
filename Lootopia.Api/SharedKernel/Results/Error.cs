namespace Lootopia.Api.SharedKernel.Results;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NotFound = new("General.NotFound", "The requested resource was not found.");
    public static readonly Error Validation = new("General.Validation", "A validation error occurred.");
    public static readonly Error Conflict = new("General.Conflict", "A conflict occurred.");
    public static readonly Error Forbidden = new("General.Forbidden", "Access denied.");
    public static readonly Error Unauthorized = new("General.Unauthorized", "Authentication required.");

    public static Error Custom(string code, string description) => new(code, description);
}
