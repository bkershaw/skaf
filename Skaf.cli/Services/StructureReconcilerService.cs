using Skaf.cli.YamlStructureModels;

namespace Skaf.cli.Services;

public static class StructureReconcilerService
{
    public static ReconciliationResult Reconcile(GeneratedStructureSummary? previous, GeneratedStructureSummary current)
    {
        var result = new ReconciliationResult();

        previous ??= new GeneratedStructureSummary();

        // Reconcile Services
        var prevServices = previous.Services.ToDictionary(s => (s.Name, s.Directory));
        var currServices = current.Services.ToDictionary(s => (s.Name, s.Directory));

        foreach (var curr in currServices.Values)
        {
            if (curr.Projects.Count == 0 && prevServices.TryGetValue((curr.Name, curr.Directory), out var prev))
            {
                curr.Projects = prev.Projects;
            }
            if (curr.TestProjects.Count == 0 && prevServices.TryGetValue((curr.Name, curr.Directory), out var prevTests))
            {
                curr.TestProjects = prevTests.TestProjects;
            }

            if (prevServices.ContainsKey((curr.Name, curr.Directory)))
                result.Unchanged.Add($"Service:{curr.Name} @ {curr.Directory}");
            else
                result.Added.Add($"Service:{curr.Name} @ {curr.Directory}");
        }
        result.ReconciledSummary.Services.AddRange(currServices.Values);

        foreach (var prev in prevServices.Values)
        {
            if (!currServices.ContainsKey((prev.Name, prev.Directory)))
                result.Removed.Add($"Service:{prev.Name} @ {prev.Directory}");
        }

        // Reconcile Components
        var prevComponents = previous.Components.ToDictionary(c => (c.Name, c.Directory));
        var currComponents = current.Components.ToDictionary(c => (c.Name, c.Directory));

        foreach (var curr in currComponents.Values)
        {
            if (curr.Projects.Count == 0 && prevComponents.TryGetValue((curr.Name, curr.Directory), out var prev))
            {
                curr.Projects = prev.Projects;
            }
            if (curr.TestProjects.Count == 0 && prevComponents.TryGetValue((curr.Name, curr.Directory), out var prevTests))
            {
                curr.TestProjects = prevTests.TestProjects;
            }

            if (prevComponents.ContainsKey((curr.Name, curr.Directory)))
                result.Unchanged.Add($"Component:{curr.Name} @ {curr.Directory}");
            else
                result.Added.Add($"Component:{curr.Name} @ {curr.Directory}");
        }
        result.ReconciledSummary.Components.AddRange(currComponents.Values);

        foreach (var prev in prevComponents.Values)
        {
            if (!currComponents.ContainsKey((prev.Name, prev.Directory)))
                result.Removed.Add($"Component:{prev.Name} @ {prev.Directory}");
        }

        // Reconcile WebApps
        var prevWeb = previous.WebApps.ToDictionary(w => (w.Name, w.Directory));
        var currWeb = current.WebApps.ToDictionary(w => (w.Name, w.Directory));

        foreach (var curr in currWeb.Values)
        {
            if (prevWeb.ContainsKey((curr.Name, curr.Directory)))
                result.Unchanged.Add($"WebApp:{curr.Name} @ {curr.Directory}");
            else
                result.Added.Add($"WebApp:{curr.Name} @ {curr.Directory}");
        }
        result.ReconciledSummary.WebApps.AddRange(currWeb.Values);

        foreach (var prev in prevWeb.Values)
        {
            if (!currWeb.ContainsKey((prev.Name, prev.Directory)))
                result.Removed.Add($"WebApp:{prev.Name} @ {prev.Directory}");
        }

        return result;
    }
}