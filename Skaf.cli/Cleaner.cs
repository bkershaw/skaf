namespace Skaf.cli;

public static class Cleaner
{
    public static void Run(string filePath)
    {
        // var structure = YamlParser.LoadStructureDefinition(filePath);
        //
        // var targets = new List<string>();
        //
        // if (structure.Services is not null)
        //     targets.AddRange(structure.Services.Select(s => s.SolutionName));
        //
        // if (structure.Components is not null)
        //     targets.AddRange(structure.Components.Select(c => c.SolutionName));
        //
        // if (structure.Web is not null)
        //     targets.AddRange(structure.Web.Select(w => w.SolutionName));
        //
        // foreach (var target in targets.Distinct())
        // {
        //     var path = Path.Combine(rootDirectory, target);
        //     if (Directory.Exists(path))
        //     {
        //         Directory.Delete(path, recursive: true);
        //         Console.WriteLine($"Deleted folder: {path}");
        //     }
        //
        //     var slnPath = Path.Combine(rootDirectory, $"{target}.sln");
        //     if (File.Exists(slnPath))
        //     {
        //         File.Delete(slnPath);
        //         Console.WriteLine($"Deleted solution: {slnPath}");
        //     }
        // }

        Console.WriteLine("Clean complete.");
    }
}