using System.Text;
using Skaf.cli.YamlStructureModels;

namespace Skaf.cli.Services;

public static class FileTreeRendererService
{
    public static string Render(StructureDefinition structure, GeneratedStructureSummary? previous = null)
    {
        var sb = new StringBuilder();
        bool anyChanges = false;

        // SERVICES
        if (structure.Services != null)
        {
            sb.AppendLine("Services/");
            foreach (var service in structure.Services)
            {
                var previousService = previous?.Services?.FirstOrDefault(p => p.Name == service.Name);
                bool isNewService = previous?.Services == null || !previous.Services.Any(p => p.Name == service.Name);

                // Mark root folder with [+] if new
                if (isNewService)
                {
                    sb.AppendLine($"\x1b[32m└── [+] {service.Name}/\x1b[0m");
                    // Render inner structure with previous = null to mark all inside as new
                    var changed = RenderDotnetStructure(sb, service.Name, "Services", service.Src.Projects, service.Tests, null, skipRoot: true);
                    anyChanges = true;
                }
                else
                {
                    var changed = RenderDotnetStructure(sb, service.Name, "Services", service.Src.Projects, service.Tests, previousService);
                    if (changed) anyChanges = true;
                }
            }
            var removedServices = previous?.Services?.Where(p => structure.Services.All(s => s.Name != p.Name)) ?? Enumerable.Empty<GeneratedService>();
            foreach (var removedService in removedServices)
            {
                anyChanges = true;
                RenderDotnetStructure(
                    sb,
                    removedService.Name,
                    "Services",
                    new List<ProjectDefinition>(),
                    null,
                    previousProjects: removedService.Projects,
                    previousTestProjects: removedService.TestProjects,
                    skipRoot: false
                );
            }
        }

        // COMPONENTS
        if (structure.Components != null)
        {
            sb.AppendLine("Components/");
            foreach (var component in structure.Components)
            {
                var previousComponent = previous?.Components?.FirstOrDefault(p => p.Name == component.Name);
                bool isNewComponent = previous?.Components == null || !previous.Components.Any(p => p.Name == component.Name);

                if (isNewComponent)
                {
                    sb.AppendLine($"\x1b[32m└── [+] {component.Name}/\x1b[0m");
                    var changed = RenderDotnetStructure(
                        sb,
                        component.Name,
                        "Components",
                        component.Src.Projects,
                        component.Tests,
                        null,
                        skipRoot: true);
                    anyChanges = true;
                }
                else
                {
                    var changed = RenderDotnetStructure(
                        sb,
                        component.Name,
                        "Components",
                        component.Src.Projects,
                        component.Tests,
                        previousComponent?.Projects,
                        previousComponent?.TestProjects);
                    if (changed) anyChanges = true;
                }
            }
            var removedComponents = (previous?.Components?.Where(p => structure.Components.All(c => c.Name != p.Name)) ?? Enumerable.Empty<GeneratedComponent>());
            foreach (var removedComponent in removedComponents)
            {
            anyChanges = true;
            RenderDotnetStructure(
                sb,
                removedComponent.Name,
                "Components",
                new List<ProjectDefinition>(),
                null,
                previousProjects: removedComponent.Projects,
                previousTestProjects: removedComponent.TestProjects,
                skipRoot: false
            );
            }
        }

        // WEB
        if (structure.Web != null)
        {
            sb.AppendLine("Web/");
            foreach (var webApp in structure.Web)
            {
                bool isNewWebApp = previous?.WebApps == null || !previous.WebApps.Any(w => w.Name == webApp.Name);

                if (isNewWebApp)
                {
                    sb.AppendLine($"\x1b[32m└── [+] {webApp.Name}/\x1b[0m");
                    anyChanges = true;
                     // sb.AppendLine($"    ├── {webApp.SolutionName}/");
                    sb.AppendLine("\x1b[32m    └── [web files generated via template]\x1b[0m");
                }
                else
                {
                    sb.AppendLine($"└── {webApp.Name}/");
                    sb.AppendLine($"    ├── {webApp.SolutionName}/");
                    sb.AppendLine($"    └── [web files generated via template]");
                }
            }
            var removedWebApps = previous?.WebApps?.Where(w => structure.Web.All(c => c.Name != w.Name)) ?? Enumerable.Empty<GeneratedWebApp>();
            foreach (var removedWebApp in removedWebApps)
            {
                anyChanges = true;
                sb.AppendLine($"\x1b[31m└── [-] {removedWebApp.Name}/\x1b[0m");
            }
        }

        // DEVOPS
        if (structure.Devops?.Pipelines?.Any() == true)
        {
            sb.AppendLine("DevOps/");
            foreach (var pipeline in structure.Devops.Pipelines)
            {
                bool isNewPipeline = true; //previous?.Devops?.Pipelines == null || !previous.Devops.Pipelines.Contains(pipeline);

                if (isNewPipeline)
                {
                    sb.AppendLine($"\x1b[32m└── [+] {pipeline}\x1b[0m");
                    anyChanges = true;
                }
                else
                {
                    sb.AppendLine($"└── {pipeline}");
                }
            }
            // var removedPipelines = previous?.Devops?.Pipelines?.Where(p => !structure.Devops.Pipelines.Contains(p)) ?? Enumerable.Empty<string>();
            // foreach (var removedPipeline in removedPipelines)
            // {
            //     anyChanges = true;
            //     sb.AppendLine($"\x1b[31m└── [-] {removedPipeline}\x1b[0m");
            // }
        }

        if (previous != null && !anyChanges)
        {
            sb.AppendLine();
            sb.AppendLine("No changes detected compared to previous structure.");
        }

        return sb.ToString();
    }

    private static bool RenderDotnetStructure(
        StringBuilder sb,
        string name,
        string rootFolder,
        List<ProjectDefinition> projects,
        TestDefinition? tests,
        GeneratedService? previous = null,
        bool skipRoot = false
    )
    {
        var indent = "└── ";
        var subIndent = "    ";
        bool anyChanges = false;

        var previousProjects = previous?.Projects ?? new List<string>();
        var previousTestProjects = previous?.TestProjects ?? new List<string>();

        if (!skipRoot) sb.AppendLine($"{indent}{name}/");
        var isNewSolution = previousProjects.Count == 0 && projects.Count > 0;
        sb.AppendLine(isNewSolution
            ? $"\x1b[32m{subIndent}├── [+] {name}.sln\x1b[0m"
            : $"{subIndent}├── {name}.sln");
        sb.AppendLine(isNewSolution
            ? $"\x1b[32m{subIndent}├── [+] src/\x1b[0m"
            : $"{subIndent}├── src/");

        var currentNames = projects.Select(p => p.Name).ToHashSet();
        foreach (var project in projects)
        {
            var isNewProject = !previousProjects.Contains(project.Name);
            var line = $"{subIndent}│   └── {project.Name}/";
            if (isNewProject)
            {
                anyChanges = true;
                sb.AppendLine($"\x1b[32m{subIndent}│   └── [+] {project.Name}/\x1b[0m");
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        var removed = previousProjects.Where(p => !currentNames.Contains(p));
        foreach (var rem in removed)
        {
            anyChanges = true;
            sb.AppendLine($"{subIndent}│   └── \x1b[31m[-] {rem}/\x1b[0m");
        }

        var isNewTests = previousTestProjects.Count == 0 && tests?.GenerateForEachProject == true;
        if (tests?.GenerateForEachProject == true)
        {
            sb.AppendLine(isNewTests
                ? $"\x1b[32m{subIndent}└── [+] tests/\x1b[0m"
                : $"{subIndent}└── tests/");
            var currentTestNames = projects.Select(p => $"{p.Name}.UnitTests").ToHashSet();
            foreach (var project in projects)
            {
                var testName = $"{project.Name}.UnitTests";
                var isNewTest = !previousTestProjects.Contains(testName);
                var line = $"{subIndent}    └── {testName}/";
                if (isNewTest)
                {
                    anyChanges = true;
                    sb.AppendLine($"\x1b[32m{subIndent}    └── [+] {testName}/\x1b[0m");
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
        }

        if (tests?.GenerateForEachProject == true)
        {
            var currentTestNames = projects.Select(p => $"{p.Name}.UnitTests").ToHashSet();
            var removedTests = previousTestProjects.Where(p => !currentTestNames.Contains(p));
            foreach (var rem in removedTests)
            {
                anyChanges = true;
                sb.AppendLine($"{subIndent}    └── \x1b[31m[-] {rem}/\x1b[0m");
            }
        }

        return anyChanges;
    }

    private static bool RenderDotnetStructure(
        StringBuilder sb,
        string name,
        string rootFolder,
        List<ProjectDefinition> projects,
        TestDefinition? tests,
        List<string>? previousProjects,
        List<string>? previousTestProjects,
        bool skipRoot = false
    )
    {
        var indent = "└── ";
        var subIndent = "    ";
        bool anyChanges = false;

        var isRemoved = projects.Count == 0 && previousProjects?.Count > 0;

        if (!skipRoot)
        {
            sb.AppendLine(isRemoved
                ? $"\x1b[31m{indent}[-] {name}/\x1b[0m"
                : $"{indent}{name}/");
        }
        sb.AppendLine(isRemoved
            ? $"\x1b[31m{subIndent}├── [-] {name}.sln\x1b[0m"
            : (projects.Count > 0 ? $"{subIndent}├── {name}.sln" : ""));
        sb.AppendLine(isRemoved
            ? $"\x1b[31m{subIndent}├── [-] src/\x1b[0m"
            : (projects.Count > 0 ? $"{subIndent}├── src/" : ""));

        var currentNames = projects.Select(p => p.Name).ToHashSet();
        foreach (var project in projects)
        {
            var isNewProject = previousProjects?.Contains(project.Name) != true;
            var line = $"{subIndent}│   └── {project.Name}/";
            if (isNewProject)
            {
                anyChanges = true;
                sb.AppendLine($"\x1b[32m{subIndent}│   └── [+] {project.Name}/\x1b[0m");
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        // Always check for removed projects if previousProjects exists, even if projects is empty
        var removed = previousProjects?.Where(p => !currentNames.Contains(p)) ?? Enumerable.Empty<string>();
        foreach (var rem in removed)
        {
            anyChanges = true;
            sb.AppendLine($"\x1b[31m{subIndent}│   └── [-] {rem}/\x1b[0m");
        }

        var isNewTests = (previousTestProjects?.Count ?? 0) == 0 && tests?.GenerateForEachProject == true;
        if (tests?.GenerateForEachProject == true)
        {
            sb.AppendLine(isNewTests
                ? $"\x1b[32m{subIndent}└── [+] tests/\x1b[0m"
                : $"{subIndent}└── tests/");
            foreach (var project in projects)
            {
                var testName = $"{project.Name}.UnitTests";
                var isNewTest = previousTestProjects?.Contains(testName) != true;
                var line = $"{subIndent}    └── {testName}/";
                if (isNewTest)
                {
                    anyChanges = true;
                    sb.AppendLine($"\x1b[32m{subIndent}    └── [+] {testName}/\x1b[0m");
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
        }

        // Always check for removed test projects if previousTestProjects exists, even if projects is empty
        if (previousTestProjects != null)
        {
            var currentTestNames = projects.Select(p => $"{p.Name}.UnitTests").ToHashSet();
            var removedTests = previousTestProjects.Where(p => !currentTestNames.Contains(p)).ToList();
            if (removedTests.Any())
            {
                anyChanges = true;
                sb.AppendLine($"\x1b[31m{subIndent}└── [-] tests/\x1b[0m");
                foreach (var rem in removedTests)
                {
                    sb.AppendLine($"\x1b[31m{subIndent}    └── [-] {rem}/\x1b[0m");
                }
            }
        }

        return anyChanges;
    }
}