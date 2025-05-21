using System.CommandLine;
using Skaf.cli.Constants;
using Skaf.cli.Services;

namespace Skaf.cli.Commands;

public static class InitializeCommand
{
    public static Command Create()
    {
        var initCommand = new Command("init", $"Create example {Globals.DefaultStructureFileName} file");

        var pathOption = new Option<string>(
            "--path",
            () => Directory.GetCurrentDirectory(),
            $"The output directory for {Globals.DefaultStructureFileName}"
        );

        initCommand.AddOption(pathOption);
        initCommand.SetHandler(InitializeStructureService.Create, pathOption);
        
        return initCommand;
    }
}