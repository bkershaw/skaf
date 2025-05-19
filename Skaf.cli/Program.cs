using System.CommandLine;
using Skaf.cli;

var rootCommand = new RootCommand("Usage: scaf [command] [options]");

var fileOption = new Option<FileInfo?>(
    name: "--file",
    description: "Path to structure YAML file",
    getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "structure.yaml"))
);

// build command
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
        
        var yamlPath = file?.FullName ?? Path.Combine(Directory.GetCurrentDirectory(), "structure.yaml");
        var baseDir = Path.GetDirectoryName(yamlPath)!;
        
        var previousSummary = StructureSummaryLoader.LoadFromFile(baseDir);
        
        var tree = FileTreeRenderer.Render(structure, previousSummary);
        Console.WriteLine(tree);

        Console.Write("Proceed with generation? (y/n): ");
        var response = Console.ReadLine();
        if (response?.ToLower() != "y")
        {
            Console.WriteLine("🚫  Build cancelled by user.");
            return;
        }

        
        Console.WriteLine("⚙️ Generating...");
        var result = StructureGenerator.Generate(structure, baseDir);
        //todo result will be diff if no changes made to project or removal of projects, resulting in skaf.json
        // file being updated with incorrect structure.
        var outputPath = Path.Combine(baseDir, "skaf.json");
        JsonFileWriter.Write(outputPath, result);

        Console.WriteLine("✅  Build completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Build failed: {ex.Message}");
    }
}, fileOption);

// clean command
var cleanCommand = new Command("clean", "Remove generated artifacts");
cleanCommand.AddOption(fileOption);
cleanCommand.SetHandler((FileInfo? file) =>
{
 
    Console.WriteLine($"❌ Clean failed: ");
 
}, fileOption);

var validateCommand = new Command("validate", "Validate YAML structure and dependencies");
validateCommand.AddOption(fileOption);
validateCommand.SetHandler((FileInfo? file) =>
{
    if (file is null)
    {
        Console.WriteLine("file cannot be null when using --file option.");
    }
    
    Console.WriteLine($"Validating {file?.FullName}...");
    StructureValidator.Validate(file?.FullName);
}, fileOption);

// init command
// ex: new[] { "init", "--path", "/Users/10358511/desktop" }
var initCommand = new Command("init", "Create example structure.yaml file");

var pathOption = new Option<string>(
    "--path",
    () => Directory.GetCurrentDirectory(),
    "The output directory for structure.yaml"
);

initCommand.AddOption(pathOption);

initCommand.SetHandler(InitializeExample.Create, pathOption);

// Add all subcommands to root
rootCommand.AddCommand(buildCommand);
rootCommand.AddCommand(cleanCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(initCommand);

// Run
//args
await rootCommand.InvokeAsync(new[] { "build", "--file", "/Users/10358511/desktop/structure/structure.yaml"});