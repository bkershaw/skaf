using System.Text;
using System.Xml.Linq;

namespace Skaf.cli;

using YamlStructureModels;
using System.Diagnostics;

public class GeneratedStructureSummary
{
    public List<GeneratedService> Services { get; set; } = new();
    public List<GeneratedComponent> Components { get; set; } = new();
    public List<GeneratedWebApp> WebApps { get; set; } = new();
}

public class GeneratedService
{
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public List<string> Projects { get; set; } = new();
    public List<string> TestProjects { get; set; } = new();
}

public class GeneratedComponent
{
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public List<string> Projects { get; set; } = new();
    public List<string> TestProjects { get; set; } = new();
}

public class GeneratedWebApp
{
    public string Name { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
}

public static class StructureGenerator
{
    public static GeneratedStructureSummary Generate(StructureDefinition structure, string baseDirectory)
    {
        var summary = new GeneratedStructureSummary();

        //todo depends on should use project references properly but its important to create projects first in the right order
        if (structure.Services != null)
        {
            foreach (var service in structure.Services)
            {
                var result = GenerateDotnetSolution(Path.Combine(baseDirectory, "Services"), service.Name, service.SolutionName, service.Src, service.Tests);
                summary.Services.Add(new GeneratedService
                {
                    Name = service.Name,
                    Directory = result.Directory,
                    Projects = result.Projects,
                    TestProjects = result.TestProjects
                });
            }
        }
        
        if (structure.Components != null)
        {
            foreach (var component in structure.Components)
            {
                var result = GenerateDotnetSolution(Path.Combine(baseDirectory, "Components"), component.Name, component.SolutionName, component.Src, component.Tests);
                summary.Components.Add(new GeneratedComponent
                {
                    Name = component.Name,
                    Directory = result.Directory,
                    Projects = result.Projects,
                    TestProjects = result.TestProjects
                });
            }
        }

        if (structure.Web != null)
        {
            foreach (var web in structure.Web)
            {
                var webPath = Path.Combine(baseDirectory, "Web", web.Name);
                GenerateWebApp(web, Path.Combine(baseDirectory, "Web"));

                summary.WebApps.Add(new GeneratedWebApp
                {
                    Name = web.Name,
                    Directory = webPath
                });
            }
        }

        // DevOps or other features like JSON metadata file can be handled here later

        return summary;
    }

    private static (string Directory, List<string> Projects, List<string> TestProjects) GenerateDotnetSolution(string categoryRoot, string folderName, string solutionName, SrcDefinition src, TestDefinition? tests)
    {
        var rootPath = Path.Combine(categoryRoot, folderName);
        var srcPath = Path.Combine(rootPath, "src");
        var testPath = Path.Combine(rootPath, "tests");

        Directory.CreateDirectory(srcPath);

        var solutionFile = Path.Combine(rootPath, $"{solutionName}.sln");
        if (!File.Exists(solutionFile))
        {
            Run("dotnet", $"new sln -n {solutionName}", rootPath);
        }

        var createdProjects = new List<string>();
        var createdTestProjects = new List<string>();

        foreach (var project in src.Projects)
        {
            var projectPath = Path.Combine(srcPath, project.Name);
            if (!Directory.Exists(projectPath))
            {
                Run("dotnet", $"new {project.Template} -n {project.Name} -f {project.Version}", srcPath);
                
                // Set RootNamespace in .csproj
                var csprojFile = Path.Combine(projectPath, $"{project.Name}.csproj");
                PatchCsprojWithNamespace(csprojFile, src.Namespace, project.Name);

                // Add project to solution using absolute path
                Run("dotnet", $"sln add \"{csprojFile}\"", rootPath);

                createdProjects.Add(project.Name);

                // Install NuGet packages
                if (project.Packages != null)
                {
                    foreach (var pkg in project.Packages)
                    {
                        var versionArg = string.IsNullOrWhiteSpace(pkg.Version) ? "" : $" -v {pkg.Version}";
                        //Run("dotnet", $"add package {pkg.Name}{versionArg}", projectPath);
                    }
                }
            }
        }

        if (tests?.GenerateForEachProject == true)
        {
            Directory.CreateDirectory(testPath);

            foreach (var project in src.Projects)
            {
                var testProjectName = $"{project.Name}.UnitTests";
                var testProjectPath = Path.Combine(testPath, testProjectName);

                if (!Directory.Exists(testProjectPath))
                {
                    Run("dotnet", $"new {tests.Template} -n {testProjectName}", testPath);

                    var testCsprojFilePath = Path.Combine(testProjectPath, $"{testProjectName}.csproj");

                    PatchCsprojWithNamespace(testCsprojFilePath, src.Namespace, project.Name);

                    Run("dotnet", $"sln add \"{testCsprojFilePath}\"", rootPath);

                    createdTestProjects.Add(testProjectName);
                }
            }
        }

        return (rootPath, createdProjects, createdTestProjects);
    }

    private static void GenerateWebApp(WebApp web, string webRoot)
    {
        var webPath = Path.Combine(webRoot, web.Name);
        if (Directory.Exists(webPath))
        {
            return;
        }
        Directory.CreateDirectory(webPath);

        if (!string.IsNullOrWhiteSpace(web.Command))
        {
            RunShell(web.Command, webPath);
        }

        if (web.PostCreate != null)
        {
            foreach (var cmd in web.PostCreate)
            {
                RunShell(cmd, webPath);
            }
        }
    }

    private static void Run(string fileName, string args, string? workingDir = null)
    {
        Console.WriteLine($"> {fileName} {args}");

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        var process = Process.Start(psi)!;
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Command failed: {fileName} {args}");
            Console.WriteLine(process.StandardError.ReadToEnd());
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }

    private static void RunShell(string command, string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true, // Important: prevent interactive hang
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (_, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Important: close stdin to avoid waiting for input
        process.StandardInput.Close();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Shell command failed: {command}");
            Console.WriteLine(errorBuilder.ToString());
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(outputBuilder.ToString());
        }
    }
    
    private static void PatchCsprojWithNamespace(string csprojPath, string namespaceBase, string projectName)
    {
        var fullNamespace = $"{namespaceBase}.{projectName}";

        var doc = XDocument.Load(csprojPath);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

        // Find or create PropertyGroup
        var propertyGroup = doc.Root?
            .Elements(ns + "PropertyGroup")
            .FirstOrDefault();

        if (propertyGroup == null)
        {
            propertyGroup = new XElement(ns + "PropertyGroup");
            doc.Root?.AddFirst(propertyGroup);
        }

        // Add or update AssemblyName and RootNamespace
        propertyGroup.SetElementValue(ns + "AssemblyName", fullNamespace);
        propertyGroup.SetElementValue(ns + "RootNamespace", fullNamespace);

        doc.Save(csprojPath);
    }
}