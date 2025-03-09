using System.Text;
using System.Text.Json;
using JsonConverterTool.Models;

namespace JsonConverterTool.Services
{
    public class JsonToCSharpConverterService
    {
        public string ConvertJsonToCSharp(string jsonContent, string rootClassName, bool generateProperties, bool generateJsonAttributes, bool usePascalCase)
        {
            try
            {
                // Parse the JSON to get its structure
                JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                
                // Generate C# classes based on the JSON structure
                StringBuilder codeBuilder = new StringBuilder();
                
                // Add necessary using statements
                codeBuilder.AppendLine("using System;");
                codeBuilder.AppendLine("using System.Collections.Generic;");
                
                if (generateJsonAttributes)
                {
                    codeBuilder.AppendLine("using System.Text.Json.Serialization;");
                }
                
                codeBuilder.AppendLine();
                codeBuilder.AppendLine("namespace GeneratedNamespace");
                codeBuilder.AppendLine("{");
                
                // Generate the root class
                GenerateClass(codeBuilder, jsonDocument.RootElement, rootClassName, 1, generateProperties, generateJsonAttributes, usePascalCase);
                
                codeBuilder.AppendLine("}");
                
                return codeBuilder.ToString();
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting JSON to C#: {ex.Message}", ex);
            }
        }
        
        private void GenerateClass(StringBuilder codeBuilder, JsonElement element, string className, int indentLevel, bool generateProperties, bool generateJsonAttributes, bool usePascalCase)
        {
            string indent = new string(' ', indentLevel * 4);
            
            codeBuilder.AppendLine($"{indent}public class {className}");
            codeBuilder.AppendLine($"{indent}{{");
            
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string propertyName = property.Name;
                    string propertyType = GetCSharpType(property.Value, usePascalCase ? ToPascalCase(propertyName) : propertyName);
                    
                    // Generate JSON attribute if needed
                    if (generateJsonAttributes)
                    {
                        codeBuilder.AppendLine($"{indent}    [JsonPropertyName(\"{propertyName}\")]");
                    }
                    
                    // Generate property
                    if (generateProperties)
                    {
                        codeBuilder.AppendLine($"{indent}    public {propertyType} {(usePascalCase ? ToPascalCase(propertyName) : propertyName)} {{ get; set; }}");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"{indent}    public {propertyType} {(usePascalCase ? ToPascalCase(propertyName) : propertyName)};");
                    }
                    
                    // If the property is an object, generate a nested class
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        codeBuilder.AppendLine();
                        GenerateClass(codeBuilder, property.Value, propertyType, indentLevel + 1, generateProperties, generateJsonAttributes, usePascalCase);
                    }
                    // If the property is an array of objects, generate a nested class for the array items
                    else if (property.Value.ValueKind == JsonValueKind.Array && property.Value.GetArrayLength() > 0 && property.Value[0].ValueKind == JsonValueKind.Object)
                    {
                        string itemClassName = propertyType.Replace("List<", "").Replace(">", "");
                        codeBuilder.AppendLine();
                        GenerateClass(codeBuilder, property.Value[0], itemClassName, indentLevel + 1, generateProperties, generateJsonAttributes, usePascalCase);
                    }
                }
            }
            
            codeBuilder.AppendLine($"{indent}}}");
        }
        
        private string GetCSharpType(JsonElement element, string propertyName)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return "string";
                case JsonValueKind.Number:
                    // Try to determine if it's an integer or a decimal
                    if (element.TryGetInt32(out _))
                        return "int";
                    else if (element.TryGetInt64(out _))
                        return "long";
                    else
                        return "decimal";
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return "bool";
                case JsonValueKind.Object:
                    // For objects, use the property name as the type
                    return ToPascalCase(propertyName);
                case JsonValueKind.Array:
                    // For arrays, determine the type of the items
                    if (element.GetArrayLength() > 0)
                    {
                        string itemType = GetCSharpType(element[0], GetSingular(propertyName));
                        return $"List<{itemType}>";
                    }
                    return "List<object>";
                case JsonValueKind.Null:
                    return "object";
                default:
                    return "object";
            }
        }
        
        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            // Handle special cases like IDs
            if (input.Equals("id", StringComparison.OrdinalIgnoreCase))
                return "Id";
            
            if (input.EndsWith("id", StringComparison.OrdinalIgnoreCase) && input.Length > 2)
                return ToPascalCase(input.Substring(0, input.Length - 2)) + "Id";
            
            // Split by non-alphanumeric characters
            string[] parts = System.Text.RegularExpressions.Regex.Split(input, "[^a-zA-Z0-9]");
            
            StringBuilder result = new StringBuilder();
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    result.Append(char.ToUpper(part[0]) + (part.Length > 1 ? part.Substring(1) : ""));
                }
            }
            
            return result.ToString();
        }
        
        private string GetSingular(string plural)
        {
            // Very basic pluralization handling
            if (plural.EndsWith("ies"))
                return plural.Substring(0, plural.Length - 3) + "y";
            else if (plural.EndsWith("s") && !plural.EndsWith("ss"))
                return plural.Substring(0, plural.Length - 1);
            else
                return plural;
        }
    }
} 