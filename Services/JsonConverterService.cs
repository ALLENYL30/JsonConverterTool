using JsonConverterTool.Models;
using System.Text.Json;

namespace JsonConverterTool.Services
{
    public class JsonConverterService : IJsonConverterService
    {
        private readonly JsonToCSharpConverterService _csharpConverter;
        private readonly JsonToJavaConverterService _javaConverter;
        private readonly XmlJsonConverterService _xmlJsonConverter;
        private readonly YamlJsonConverterService _yamlJsonConverter;

        public JsonConverterService()
        {
            _csharpConverter = new JsonToCSharpConverterService();
            _javaConverter = new JsonToJavaConverterService();
            _xmlJsonConverter = new XmlJsonConverterService();
            _yamlJsonConverter = new YamlJsonConverterService();
        }

        public async Task<JsonConversionResponse> ConvertJsonToCodeAsync(JsonConversionRequest request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return new JsonConversionResponse
                    {
                        Success = false,
                        ErrorMessage = "Content cannot be empty",
                        Language = request.TargetFormat
                    };
                }

                string generatedCode;
                string sourceContent = request.Content;

                try
                {
                    // Convert source format to JSON if needed
                    if (request.SourceFormat.ToLower() != "json")
                    {
                        sourceContent = await ConvertToJsonAsync(request.Content, request.SourceFormat);
                    }

                    // Validate JSON if source is JSON or we've converted to JSON
                    if (request.SourceFormat.ToLower() == "json" || request.TargetFormat.ToLower() != "json")
                    {
                        // Try to parse JSON to validate it
                        try
                        {
                            JsonDocument.Parse(sourceContent);
                        }
                        catch (JsonException ex)
                        {
                            return new JsonConversionResponse
                            {
                                Success = false,
                                ErrorMessage = $"Invalid JSON format: {ex.Message}",
                                Language = request.TargetFormat
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new JsonConversionResponse
                    {
                        Success = false,
                        ErrorMessage = $"Error processing source content: {ex.Message}",
                        Language = request.TargetFormat
                    };
                }

                // Convert JSON to the target format
                switch (request.TargetFormat.ToLower())
                {
                    case "csharp":
                    case "c#":
                        generatedCode = _csharpConverter.ConvertJsonToCSharp(
                            sourceContent,
                            request.RootClassName,
                            request.GenerateProperties,
                            request.GenerateJsonAttributes,
                            request.UsePascalCase
                        );
                        break;

                    case "java":
                        generatedCode = _javaConverter.ConvertJsonToJava(
                            sourceContent,
                            request.RootClassName,
                            request.GenerateProperties,
                            request.GenerateJsonAttributes,
                            request.UsePascalCase
                        );
                        break;

                    case "xml":
                        generatedCode = _xmlJsonConverter.ConvertJsonToXml(sourceContent);
                        break;

                    case "yaml":
                    case "yml":
                        try
                        {
                            generatedCode = _yamlJsonConverter.ConvertJsonToYaml(sourceContent);
                        }
                        catch (Exception ex)
                        {
                            return new JsonConversionResponse
                            {
                                Success = false,
                                ErrorMessage = $"Error converting to YAML: {ex.Message}",
                                Language = request.TargetFormat
                            };
                        }
                        break;

                    case "json":
                        // If source and target are both JSON, just return the content
                        if (request.SourceFormat.ToLower() == "json")
                        {
                            generatedCode = sourceContent;
                        }
                        else
                        {
                            // This means we've already converted to JSON from another format
                            generatedCode = sourceContent;
                        }
                        break;

                    default:
                        return new JsonConversionResponse
                        {
                            Success = false,
                            ErrorMessage = $"Unsupported target format: {request.TargetFormat}",
                            Language = request.TargetFormat
                        };
                }

                return new JsonConversionResponse
                {
                    Success = true,
                    GeneratedCode = generatedCode,
                    Language = request.TargetFormat
                };
            }
            catch (Exception ex)
            {
                return new JsonConversionResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Language = request.TargetFormat
                };
            }
        }

        private async Task<string> ConvertToJsonAsync(string content, string sourceFormat)
        {
            return await Task.Run(() =>
            {
                switch (sourceFormat.ToLower())
                {
                    case "xml":
                        return _xmlJsonConverter.ConvertXmlToJson(content);
                    case "yaml":
                    case "yml":
                        return _yamlJsonConverter.ConvertYamlToJson(content);
                    default:
                        throw new ArgumentException($"Unsupported source format: {sourceFormat}");
                }
            });
        }
    }
} 