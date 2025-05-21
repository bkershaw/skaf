namespace Skaf.cli.YamlStructureModels;

public class GeneratedService
{
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public List<string> Projects { get; set; } = new();
    public List<string> TestProjects { get; set; } = new();
}