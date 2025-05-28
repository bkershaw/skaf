using System.CommandLine;
using Skaf.cli.Constants;
using Skaf.cli.Services;
using Skaf.cli.Utilities;

namespace Skaf.cli.Commands;

public static class BuildCommand
{
    public static Command Create()
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "Path to structure YAML file",
            getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Globals.DefaultStructureFileName))
        );
        
        var buildCommand = new Command("build", "Generate folder structure and project scaffolding from YAML");
        buildCommand.AddOption(fileOption);
        buildCommand.SetHandler((FileInfo? file) =>
        {
            if (file is null)
            {
                Console.WriteLine("file cannot be null when using --file option.");
                return;
            }

            try
            {
                Console.WriteLine($"📦  Parsing {file.FullName}...\n");
        
                var structure = YamlParser.Parse(file.FullName);
                Console.WriteLine("📁  Generated File Structure Preview:");
        
                var yamlPath = file?.FullName ?? Path.Combine(Directory.GetCurrentDirectory(), Globals.DefaultStructureFileName);
                var baseDir = Path.GetDirectoryName(yamlPath)!;
        
                var previousSummary = StructureSummaryLoader.LoadFromFile(baseDir);
        
                var tree = FileTreeRendererService.Render(structure, previousSummary);
                Console.WriteLine(tree);

                Console.Write("Proceed with generation? (y/n): ");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("🚫  Build cancelled by user.");
                    return;
                }

        
                Console.WriteLine("⚙️ Generating...");
                var result = StructureGenerator.Generate(structure, baseDir, previousSummary);
          
                var reconciled = StructureReconcilerService.Reconcile(previousSummary, result);

                Console.WriteLine("➕ Added: " + string.Join(", ", reconciled.Added));
                Console.WriteLine("➖ Removed: " + string.Join(", ", reconciled.Removed));
                Console.WriteLine("✔️ Unchanged: " + string.Join(", ", reconciled.Unchanged));
                
                
                var outputPath = Path.Combine(baseDir, Globals.DefaultScaffoldStructureOutputFileName);
                JsonFileWriter.Write(outputPath, reconciled.ReconciledSummary);

                Console.WriteLine("✅  Build completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Build failed: {ex.Message}");
            }
        }, fileOption);

        return buildCommand;
    }
}