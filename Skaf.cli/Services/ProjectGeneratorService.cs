using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Skaf.cli.YamlStructureModels;

namespace Skaf.cli.Services;

public static class StructureGenerator
{
    public static GeneratedStructureSummary Generate(StructureDefinition structure, string baseDirectory, GeneratedStructureSummary? previousSummary = null)
    {
        var summary = new GeneratedStructureSummary();

        //todo depends on should use project references properly but its important to create projects first in the right order
        var currentServices = structure.Services ?? new List<Service>();
        var previousServices = previousSummary?.Services ?? new List<GeneratedService>();

        foreach (var service in currentServices)
        {
            var previousService = previousServices.FirstOrDefault(s => s.Name == service.Name);
            var result = GenerateDotnetSolution(Path.Combine(baseDirectory, "Services"), service.Name, service.SolutionName, service.Src, service.Tests, previousService);
            summary.Services.Add(new GeneratedService
            {
                Name = service.Name,
                Directory = result.Directory,
                Projects = result.Projects,
                TestProjects = result.TestProjects
            });
        }

        var removedServices = previousServices.Where(p => currentServices.All(s => s.Name != p.Name));
        foreach (var removed in removedServices)
        {
            try
            {
                Directory.Delete(removed.Directory, recursive: true);
                Console.WriteLine($"🗑️ Removed service directory: {removed.Directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to remove service directory {removed.Directory}: {ex.Message}");
            }
        }
        
        var currentComponents = structure.Components ?? new List<Component>();
        var previousComponents = previousSummary?.Components ?? new List<GeneratedComponent>();

        foreach (var component in currentComponents)
        {
            var previousComponent = previousComponents.FirstOrDefault(c => c.Name == component.Name);
            var result = GenerateDotnetSolution(Path.Combine(baseDirectory, "Components"), component.Name, component.SolutionName, component.Src, component.Tests, previousComponent);
            summary.Components.Add(new GeneratedComponent
            {
                Name = component.Name,
                Directory = result.Directory,
                Projects = result.Projects,
                TestProjects = result.TestProjects
            });
        }

        var removedComponents = previousComponents.Where(p => currentComponents.All(c => c.Name != p.Name));
        foreach (var removed in removedComponents)
        {
            try
            {
                Directory.Delete(removed.Directory, recursive: true);
                Console.WriteLine($"🗑️ Removed component directory: {removed.Directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to remove component directory {removed.Directory}: {ex.Message}");
            }
        }

        var currentWebApps = structure.Web ?? new List<WebApp>();
        var previousWebApps = previousSummary?.WebApps ?? new List<GeneratedWebApp>();

        foreach (var web in currentWebApps)
        {
            var webPath = Path.Combine(baseDirectory, "Web", web.Name);
            GenerateWebApp(web, Path.Combine(baseDirectory, "Web"));

            summary.WebApps.Add(new GeneratedWebApp
            {
                Name = web.Name,
                Directory = webPath
            });
        }

        var removedWebApps = previousWebApps.Where(p => currentWebApps.All(w => w.Name != p.Name));
        foreach (var removed in removedWebApps)
        {
            try
            {
                Directory.Delete(removed.Directory, recursive: true);
                Console.WriteLine($"🗑️ Removed web app directory: {removed.Directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to remove web app directory {removed.Directory}: {ex.Message}");
            }
        }

        // DevOps or other features like JSON metadata file can be handled here later

        return summary;
    }

    private static (string Directory, List<string> Projects, List<string> TestProjects) GenerateDotnetSolution(string categoryRoot, string folderName, string solutionName, SrcDefinition src, TestDefinition? tests, GeneratedService? previous = null)
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

        {
            var previousProjectNames = previous?.Projects?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
            var desiredProjects = src.Projects.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var projectsToRemove = previousProjectNames.Except(desiredProjects);

            foreach (var toRemove in projectsToRemove)
            {
                var fullPath = Path.Combine(srcPath, toRemove);
                try
                {
                    Directory.Delete(fullPath, recursive: true);
                    Console.WriteLine($"🗑️ Removed project directory: {fullPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to remove {fullPath}: {ex.Message}");
                }
            }
        }

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

        var createdTestProjects = new List<string>();

        if (tests?.GenerateForEachProject == true)
        {
            Directory.CreateDirectory(testPath);

            {
                var previousTestProjectNames = previous?.TestProjects?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                var expectedTestProjects = src.Projects.Select(p => $"{p.Name}.UnitTests").ToHashSet(StringComparer.OrdinalIgnoreCase);
                var testsToRemove = previousTestProjectNames.Except(expectedTestProjects);

                foreach (var toRemove in testsToRemove)
                {
                    var fullPath = Path.Combine(testPath, toRemove);
                    try
                    {
                        Directory.Delete(fullPath, recursive: true);
                        Console.WriteLine($"🗑️ Removed test project directory: {fullPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Failed to remove test directory {fullPath}: {ex.Message}");
                    }
                }
            }

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
    
      private static (string Directory, List<string> Projects, List<string> TestProjects) GenerateDotnetSolution(string categoryRoot, string folderName, string solutionName, SrcDefinition src, TestDefinition? tests, GeneratedComponent? previous = null)
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

        {
            var previousProjectNames = previous?.Projects?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
            var desiredProjects = src.Projects.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var projectsToRemove = previousProjectNames.Except(desiredProjects);

            foreach (var toRemove in projectsToRemove)
            {
                var fullPath = Path.Combine(srcPath, toRemove);
                try
                {
                    Directory.Delete(fullPath, recursive: true);
                    Console.WriteLine($"🗑️ Removed project directory: {fullPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to remove {fullPath}: {ex.Message}");
                }
            }
        }

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

        var createdTestProjects = new List<string>();

        if (tests?.GenerateForEachProject == true)
        {
            Directory.CreateDirectory(testPath);

            {
                var previousTestProjectNames = previous?.TestProjects?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
                var expectedTestProjects = src.Projects.Select(p => $"{p.Name}.UnitTests").ToHashSet(StringComparer.OrdinalIgnoreCase);
                var testsToRemove = previousTestProjectNames.Except(expectedTestProjects);

                foreach (var toRemove in testsToRemove)
                {
                    var fullPath = Path.Combine(testPath, toRemove);
                    try
                    {
                        Directory.Delete(fullPath, recursive: true);
                        Console.WriteLine($"🗑️ Removed test project directory: {fullPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Failed to remove test directory {fullPath}: {ex.Message}");
                    }
                }
            }

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