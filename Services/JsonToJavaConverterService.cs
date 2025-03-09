using System.Text;
using System.Text.Json;
using JsonConverterTool.Models;

namespace JsonConverterTool.Services
{
    public class JsonToJavaConverterService
    {
        public string ConvertJsonToJava(string jsonContent, string rootClassName, bool generateProperties, bool generateJsonAttributes, bool usePascalCase)
        {
            try
            {
                // Parse the JSON to get its structure
                JsonDocument jsonDocument = JsonDocument.Parse(jsonContent);
                
                // Generate Java classes based on the JSON structure
                StringBuilder codeBuilder = new StringBuilder();
                
                // Add necessary import statements
                codeBuilder.AppendLine("import java.util.List;");
                codeBuilder.AppendLine("import java.util.ArrayList;");
                
                if (generateJsonAttributes)
                {
                    codeBuilder.AppendLine("import com.fasterxml.jackson.annotation.JsonProperty;");
                }
                
                codeBuilder.AppendLine();
                
                // Generate the root class
                GenerateClass(codeBuilder, jsonDocument.RootElement, rootClassName, 0, generateProperties, generateJsonAttributes, usePascalCase);
                
                return codeBuilder.ToString();
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting JSON to Java: {ex.Message}", ex);
            }
        }
        
        private void GenerateClass(StringBuilder codeBuilder, JsonElement element, string className, int indentLevel, bool generateProperties, bool generateJsonAttributes, bool usePascalCase)
        {
            string indent = new string(' ', indentLevel * 4);
            
            codeBuilder.AppendLine($"{indent}public class {className} {{");
            
            if (element.ValueKind == JsonValueKind.Object)
            {
                // First, declare all fields
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string propertyName = property.Name;
                    string javaPropertyName = usePascalCase ? ToCamelCase(propertyName) : propertyName;
                    string propertyType = GetJavaType(property.Value, usePascalCase ? ToPascalCase(propertyName) : propertyName);
                    
                    // Generate JSON attribute if needed
                    if (generateJsonAttributes)
                    {
                        codeBuilder.AppendLine($"{indent}    @JsonProperty(\"{propertyName}\")");
                    }
                    
                    // Generate field
                    codeBuilder.AppendLine($"{indent}    private {propertyType} {javaPropertyName};");
                }
                
                codeBuilder.AppendLine();
                
                // Generate constructor
                codeBuilder.AppendLine($"{indent}    public {className}() {{");
                codeBuilder.AppendLine($"{indent}    }}");
                codeBuilder.AppendLine();
                
                // Generate getters and setters if needed
                if (generateProperties)
                {
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        string propertyName = property.Name;
                        string javaPropertyName = usePascalCase ? ToCamelCase(propertyName) : propertyName;
                        string pascalPropertyName = ToPascalCase(propertyName);
                        string propertyType = GetJavaType(property.Value, usePascalCase ? pascalPropertyName : propertyName);
                        
                        // Getter
                        codeBuilder.AppendLine($"{indent}    public {propertyType} get{pascalPropertyName}() {{");
                        codeBuilder.AppendLine($"{indent}        return {javaPropertyName};");
                        codeBuilder.AppendLine($"{indent}    }}");
                        codeBuilder.AppendLine();
                        
                        // Setter
                        codeBuilder.AppendLine($"{indent}    public void set{pascalPropertyName}({propertyType} {javaPropertyName}) {{");
                        codeBuilder.AppendLine($"{indent}        this.{javaPropertyName} = {javaPropertyName};");
                        codeBuilder.AppendLine($"{indent}    }}");
                        codeBuilder.AppendLine();
                    }
                }
                
                // Generate nested classes
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    // If the property is an object, generate a nested class
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        string nestedClassName = ToPascalCase(property.Name);
                        codeBuilder.AppendLine();
                        GenerateClass(codeBuilder, property.Value, nestedClassName, indentLevel + 1, generateProperties, generateJsonAttributes, usePascalCase);
                    }
                    // If the property is an array of objects, generate a nested class for the array items
                    else if (property.Value.ValueKind == JsonValueKind.Array && property.Value.GetArrayLength() > 0 && property.Value[0].ValueKind == JsonValueKind.Object)
                    {
                        string itemClassName = ToPascalCase(GetSingular(property.Name));
                        codeBuilder.AppendLine();
                        GenerateClass(codeBuilder, property.Value[0], itemClassName, indentLevel + 1, generateProperties, generateJsonAttributes, usePascalCase);
                    }
                }
            }
            
            codeBuilder.AppendLine($"{indent}}}");
        }
        
        private string GetJavaType(JsonElement element, string propertyName)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return "String";
                case JsonValueKind.Number:
                    // Try to determine if it's an integer or a decimal
                    if (element.TryGetInt32(out _))
                        return "int";
                    else if (element.TryGetInt64(out _))
                        return "long";
                    else
                        return "double";
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return "boolean";
                case JsonValueKind.Object:
                    // For objects, use the property name as the type
                    return ToPascalCase(propertyName);
                case JsonValueKind.Array:
                    // For arrays, determine the type of the items
                    if (element.GetArrayLength() > 0)
                    {
                        string itemType = GetJavaType(element[0], GetSingular(propertyName));
                        return $"List<{itemType}>";
                    }
                    return "List<Object>";
                case JsonValueKind.Null:
                    return "Object";
                default:
                    return "Object";
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
        
        private string ToCamelCase(string input)
        {
            string pascalCase = ToPascalCase(input);
            if (string.IsNullOrEmpty(pascalCase))
                return pascalCase;
            
            return char.ToLower(pascalCase[0]) + (pascalCase.Length > 1 ? pascalCase.Substring(1) : "");
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