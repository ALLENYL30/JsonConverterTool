using Microsoft.AspNetCore.Mvc;
using JsonConverterTool.Models;
using JsonConverterTool.Services;

namespace JsonConverterTool.Controllers
{
    /// <summary>
    /// Controller for JSON conversion operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class JsonConverterController : ControllerBase
    {
        private readonly IJsonConverterService _jsonConverterService;
        private readonly ILogger<JsonConverterController> _logger;

        /// <summary>
        /// Initializes a new instance of the JsonConverterController
        /// </summary>
        /// <param name="jsonConverterService">The JSON converter service</param>
        /// <param name="logger">The logger</param>
        public JsonConverterController(IJsonConverterService jsonConverterService, ILogger<JsonConverterController> logger)
        {
            _jsonConverterService = jsonConverterService;
            _logger = logger;
        }

        /// <summary>
        /// Converts JSON to code in the specified language
        /// </summary>
        /// <param name="request">The JSON conversion request</param>
        /// <returns>The generated code or error message</returns>
        /// <response code="200">Returns the generated code</response>
        /// <response code="400">If the request is invalid or conversion fails</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpPost("convert")]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(JsonConversionResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JsonConversionResponse>> ConvertJson([FromBody] JsonConversionRequest request)
        {
            try
            {
                _logger.LogInformation("Received JSON conversion request for {TargetLanguage}", request.TargetLanguage);
                
                var response = await _jsonConverterService.ConvertJsonToCodeAsync(request);
                
                if (!response.Success)
                {
                    _logger.LogWarning("JSON conversion failed: {ErrorMessage}", response.ErrorMessage);
                    return BadRequest(response);
                }
                
                _logger.LogInformation("JSON conversion successful for {TargetLanguage}", request.TargetLanguage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during JSON conversion");
                return StatusCode(500, new JsonConversionResponse
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred during conversion",
                    Language = request.TargetLanguage
                });
            }
        }
    }
} 