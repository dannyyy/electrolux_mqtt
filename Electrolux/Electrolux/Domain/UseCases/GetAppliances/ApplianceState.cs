namespace Electrolux.Domain.UseCases.GetAppliances;

public record ApplianceState(IReadOnlyDictionary<string, object> States);