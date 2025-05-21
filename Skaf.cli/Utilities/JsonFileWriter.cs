using System.Text.Json;
using Skaf.cli.YamlStructureModels;

namespace Skaf.cli.Utilities;

    public static class JsonFileWriter
    {
        /// <summary>
        /// Writes an object to a JSON file at the specified path.
        /// </summary>
        /// <param name="targetFilePath">The full path to the target JSON file.</param>
        /// <param name="obj">The object to serialize.</param>
        public static void Write(string targetFilePath, GeneratedStructureSummary obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var directory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(targetFilePath, json);
        }
    }