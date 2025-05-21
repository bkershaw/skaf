using System.CommandLine;
using Skaf.cli.Constants;

namespace Skaf.cli.Commands;

public static class CleanCommand
{
    public static Command Create()
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "Path to structure YAML file",
            getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Globals.DefaultStructureFileName))
        );
        
        var cleanCommand = new Command("clean", "Remove generated artifacts");
        cleanCommand.AddOption(fileOption);
        cleanCommand.SetHandler((FileInfo? file) =>
        {
            Console.WriteLine($"❌ Clean failed: ");
 
        }, fileOption);
        
        return cleanCommand;
    }
}