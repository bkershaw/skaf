using Skaf.cli.Utilities;

namespace Skaf.cli.Services;

public static class StructureValidatorService
{
    public static void Validate(string filePath)
    {
        var structure = YamlParser.Parse(filePath);

        var errors = new List<string>();

        if (structure.Services != null && structure.Services.Any())
        {
            foreach (var service in structure.Services)
            {
                if (string.IsNullOrWhiteSpace(service.Name))
                    errors.Add("Service is missing a name.");
                if (string.IsNullOrWhiteSpace(service.SolutionName))
                    errors.Add($"Service '{service.Name}' is missing solutionName.");
                if (service.Src?.Projects == null || service.Src.Projects.Count == 0)
                    errors.Add($"Service '{service.Name}' has no projects defined.");
            }
        }

        if (structure.Components != null)
        {
            foreach (var component in structure.Components)
            {
                if (string.IsNullOrWhiteSpace(component.Name))
                    errors.Add("Component is missing a name.");
                if (string.IsNullOrWhiteSpace(component.SolutionName))
                    errors.Add($"Component '{component.Name}' is missing solutionName.");
                if (component.Src?.Projects == null || component.Src.Projects.Count == 0)
                    errors.Add($"Component '{component.Name}' has no projects defined.");
            }
        }

        if (structure.Web != null)
        {
            foreach (var web in structure.Web)
            {
                if (string.IsNullOrWhiteSpace(web.Name))
                    errors.Add("Web entry is missing a name.");
                if (string.IsNullOrWhiteSpace(web.SolutionName))
                    errors.Add($"Web '{web.Name}' is missing solutionName.");
                if (string.IsNullOrWhiteSpace(web.Command))
                    errors.Add($"Web '{web.Name}' is missing templateCommand.");
            }
        }

        if (errors.Count > 0)
        {
            Console.WriteLine("❌  Validation failed:");
            foreach (var err in errors)
                Console.WriteLine($"  - {err}");
        }
        else
        {
            Console.WriteLine("✅  YAML structure is valid.");
        }
    }
}