using System.CommandLine;
using Skaf.cli.Commands;

var rootCommand = new RootCommand("Usage: scaf [command] [options]");

rootCommand.AddCommand(CleanCommand.Create());
rootCommand.AddCommand(ValidateCommand.Create());
rootCommand.AddCommand(InitializeCommand.Create());
rootCommand.AddCommand(BuildCommand.Create());

// Run
//args
// ex: new[] { "init", "--path", "/Users/10358511/desktop" }
// new[] { "build", "--file", "/Users/blakekershaw/Desktop/example-scaf/scaf-structure.yaml"}
await rootCommand.InvokeAsync(args);