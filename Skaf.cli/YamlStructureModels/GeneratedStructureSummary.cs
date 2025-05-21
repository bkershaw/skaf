namespace Skaf.cli.YamlStructureModels;

public class GeneratedStructureSummary
{
    public List<GeneratedService> Services { get; set; } = new();
    public List<GeneratedComponent> Components { get; set; } = new();
    public List<GeneratedWebApp> WebApps { get; set; } = new();
}