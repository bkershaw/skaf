namespace Skaf.cli;
using System.Text.Json;


    public static class StructureSummaryLoader
    {
        public static GeneratedStructureSummary? LoadFromFile(string baseDir)
        {
            var path = Path.Combine(baseDir, "skaf.json");
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
                Console.WriteLine($"⚠️ Failed to read or parse skaf.json: {ex.Message}");
                return null;
            }
        }
    }
