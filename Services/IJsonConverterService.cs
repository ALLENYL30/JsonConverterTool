using JsonConverterTool.Models;

namespace JsonConverterTool.Services
{
    public interface IJsonConverterService
    {
        Task<JsonConversionResponse> ConvertJsonToCodeAsync(JsonConversionRequest request);
    }
} 