using Microsoft.AspNetCore.Mvc;
using JsonConverterTool.Models;
using JsonConverterTool.Services;

namespace JsonConverterTool.Controllers
{
    /// <summary>
    /// Controller for format conversion operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class JsonConverterController : ControllerBase
    {
        private readonly IJsonConverterService _converterService;
        private readonly ILogger<JsonConverterController> _logger;
        private readonly YamlJsonConverterService _yamlJsonConverter;

        /// <summary>
        /// Initializes a new instance of the JsonConverterController
        /// </summary>
        /// <param name="converterService">The converter service</param>
        /// <param name="logger">The logger</param>
        public JsonConverterController(IJsonConverterService converterService, ILogger<JsonConverterController> logger)
        {
            _converterService = converterService;
            _logger = logger;
            _yamlJsonConverter = new YamlJsonConverterService();
        }

        /// <summary>
        /// Converts content between different formats (JSON, XML, YAML) or to code (C#, Java)
        /// </summary>
        /// <param name="request">The conversion request</param>
        /// <returns>The converted content or error message</returns>
        /// <response code="200">Returns the converted content</response>
        /// <response code="400">If the request is invalid or conversion fails</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpPost("convert")]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JsonConversionResponse>> ConvertContent([FromBody] JsonConversionRequest request)
        {
            try
            {
                _logger.LogInformation("Received conversion request from {SourceFormat} to {TargetFormat}", 
                    request.SourceFormat, request.TargetFormat);
                
                var response = await _converterService.ConvertJsonToCodeAsync(request);
                
                if (!response.Success)
                {
                    _logger.LogWarning("Conversion failed: {ErrorMessage}", response.ErrorMessage);
                    return BadRequest(response);
                }
                
                _logger.LogInformation("Conversion successful from {SourceFormat} to {TargetFormat}", 
                    request.SourceFormat, request.TargetFormat);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during conversion");
                return StatusCode(500, new JsonConversionResponse
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred during conversion",
                    Language = request.TargetFormat
                });
            }
        }

        /// <summary>
        /// Test endpoint specifically for JSON to YAML conversion
        /// </summary>
        /// <param name="jsonContent">The JSON content to convert</param>
        /// <returns>The YAML content</returns>
        [HttpPost("test-json-to-yaml")]
        public ActionResult<string> TestJsonToYaml([FromBody] string jsonContent)
        {
            try
            {
                var yamlContent = _yamlJsonConverter.ConvertJsonToYaml(jsonContent);
                return Ok(yamlContent);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error converting JSON to YAML: {ex.Message}");
            }
        }
    }
} 