using Skaf.cli.YamlStructureModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Skaf.cli;


    public static class YamlParser
    {
        public static StructureDefinition Parse(string? filePath = null)
        {
            filePath ??= Path.Combine(Directory.GetCurrentDirectory(), "structure.yaml");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"❌  YAML file not found at: {filePath}");
            }

            try
            {
                var yaml = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var structure = deserializer.Deserialize<StructureDefinition>(yaml);

                if (structure == null)
                {
                    throw new InvalidOperationException("❌  YAML parsed to null object.");
                }

                return structure;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"❌ Failed to parse YAML file: {ex.Message}", ex);
            }
        }
    }