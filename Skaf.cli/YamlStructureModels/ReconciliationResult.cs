namespace Skaf.cli.YamlStructureModels;

public class ReconciliationResult
{
    public GeneratedStructureSummary ReconciledSummary { get; set; } = new();
    public List<string> Added { get; set; } = new();
    public List<string> Removed { get; set; } = new();
    public List<string> Unchanged { get; set; } = new();
}