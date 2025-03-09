using JsonConverterTool.Models;

namespace JsonConverterTool.Services
{
    public class JsonConverterService : IJsonConverterService
    {
        private readonly JsonToCSharpConverterService _csharpConverter;
        private readonly JsonToJavaConverterService _javaConverter;

        public JsonConverterService()
        {
            _csharpConverter = new JsonToCSharpConverterService();
            _javaConverter = new JsonToJavaConverterService();
        }

        public async Task<JsonConversionResponse> ConvertJsonToCodeAsync(JsonConversionRequest request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(request.JsonContent))
                {
                    return new JsonConversionResponse
                    {
                        Success = false,
                        ErrorMessage = "JSON content cannot be empty",
                        Language = request.TargetLanguage
                    };
                }

                string generatedCode;

                // Convert JSON to the target language
                switch (request.TargetLanguage.ToLower())
                {
                    case "csharp":
                    case "c#":
                        generatedCode = _csharpConverter.ConvertJsonToCSharp(
                            request.JsonContent,
                            request.RootClassName,
                            request.GenerateProperties,
                            request.GenerateJsonAttributes,
                            request.UsePascalCase
                        );
                        break;

                    case "java":
                        generatedCode = _javaConverter.ConvertJsonToJava(
                            request.JsonContent,
                            request.RootClassName,
                            request.GenerateProperties,
                            request.GenerateJsonAttributes,
                            request.UsePascalCase
                        );
                        break;

                    default:
                        return new JsonConversionResponse
                        {
                            Success = false,
                            ErrorMessage = $"Unsupported target language: {request.TargetLanguage}",
                            Language = request.TargetLanguage
                        };
                }

                return new JsonConversionResponse
                {
                    Success = true,
                    GeneratedCode = generatedCode,
                    Language = request.TargetLanguage
                };
            }
            catch (Exception ex)
            {
                return new JsonConversionResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Language = request.TargetLanguage
                };
            }
        }
    }
} 