using System.Text.Json;
using Skaf.cli.Constants;
using Skaf.cli.YamlStructureModels;

namespace Skaf.cli.Utilities;

public static class StructureSummaryLoader
    {
        public static GeneratedStructureSummary? LoadFromFile(string baseDir)
        {
            var path = Path.Combine(baseDir, Globals.DefaultScaffoldStructureOutputFileName);
            if (!File.Exists(path)) return null;

            try
            {
                var json = File.ReadAllText(path);
                var summary = JsonSerializer.Deserialize<GeneratedStructureSummary>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return summary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to read or parse {Globals.DefaultScaffoldStructureOutputFileName}: {ex.Message}");
                return null;
            }
        }
    }
