namespace Electrolux.Domain.UseCases.GetAppliances;

public record ApplianceCapability(
    string command,
    IReadOnlySet<string>? values = null,
    int? min = null,
    int? max = null);