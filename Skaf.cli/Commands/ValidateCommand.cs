using System.CommandLine;
using Skaf.cli.Constants;
using Skaf.cli.Services;

namespace Skaf.cli.Commands;

public static class ValidateCommand
{
    public static Command Create()
    {
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "Path to structure YAML file",
            getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Globals.DefaultStructureFileName))
        );
        
        var validateCommand = new Command("validate", "Validate YAML structure and dependencies");
        validateCommand.AddOption(fileOption);
        validateCommand.SetHandler((FileInfo? file) =>
        {
            if (file is null)
            {
                Console.WriteLine("file cannot be null when using --file option.");
            }
    
            Console.WriteLine($"Validating {file?.FullName}...");
            StructureValidatorService.Validate(file?.FullName);
        }, fileOption);
        
        return validateCommand;
    }
}