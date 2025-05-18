namespace Skaf.cli.YamlStructureModels;

public class StructureDefinition
{
    public List<Service>? Services { get; set; }
    public List<Component>? Components { get; set; }
    public List<WebApp>? Web { get; set; }
    
    public DevOpsConfig Devops { get; set; } = new();
}

public class DevOpsConfig
{
    public List<string> Pipelines { get; set; } = new();
}
public class Service
{
    public string Name { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public SrcDefinition Src { get; set; } = new();
    public TestDefinition? Tests { get; set; } 
}

public class Component
{
    public string Name { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public SrcDefinition Src { get; set; } = new();
    public TestDefinition? Tests { get; set; } 
}

public class WebApp
{
    public string Name { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<string>? PostCreate { get; set; }
}

public class SrcDefinition
{
    public string Namespace { get; set; } = string.Empty;
    public List<ProjectDefinition> Projects { get; set; } = new();
}

public class ProjectDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<string>? DependsOn { get; set; }
    public List<NugetPackage>? Packages { get; set; }
}

public class NugetPackage
{
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
}

public class TestDefinition
{
    public string? Namespace { get; set; }
    public bool GenerateForEachProject { get; set; } = false;
    public string? Template { get; set; }
}