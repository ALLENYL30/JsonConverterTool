using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JsonConverterTool.Services
{
    public class YamlJsonConverterService
    {
        public string ConvertJsonToYaml(string jsonContent)
        {
            try
            {
                // First parse the JSON using JObject to maintain the structure
                JObject jObject = JObject.Parse(jsonContent);
                
                // Create a serializer with the desired settings
                var serializer = new SerializerBuilder()
                    .DisableAliases()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .WithIndentedSequences()
                    .Build();
                
                // Convert JObject to a dictionary that YamlDotNet can handle
                var expandoObject = ConvertJTokenToDynamic(jObject);
                
                // Serialize to YAML
                string yaml = serializer.Serialize(expandoObject);
                
                return yaml;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting JSON to YAML: {ex.Message}", ex);
            }
        }
        
        // Helper method to convert JToken to dynamic objects that YamlDotNet can handle properly
        private object ConvertJTokenToDynamic(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                    foreach (var property in token.Children<JProperty>())
                    {
                        obj[property.Name] = ConvertJTokenToDynamic(property.Value);
                    }
                    return obj;
                    
                case JTokenType.Array:
                    return token.Select(item => ConvertJTokenToDynamic(item)).ToList();
                    
                case JTokenType.Integer:
                    return token.Value<long>();
                    
                case JTokenType.Float:
                    return token.Value<double>();
                    
                case JTokenType.Boolean:
                    return token.Value<bool>();
                    
                case JTokenType.Date:
                    return token.Value<DateTime>();
                    
                case JTokenType.String:
                    return token.Value<string>();
                    
                case JTokenType.Null:
                    return null;
                    
                default:
                    return token.ToString();
            }
        }

        public string ConvertYamlToJson(string yamlContent)
        {
            try
            {
                // Parse YAML to object
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yamlObject = deserializer.Deserialize<object>(yamlContent);

                // Convert object to JSON with proper formatting
                var jsonContent = JsonConvert.SerializeObject(yamlObject, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });
                
                return jsonContent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting YAML to JSON: {ex.Message}", ex);
            }
        }
    }
} 