namespace JsonConverterTool.Models
{
    /// <summary>
    /// Response model for JSON to code conversion
    /// </summary>
    public class JsonConversionResponse
    {
        /// <summary>
        /// Indicates whether the conversion was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// The generated code (if successful)
        /// </summary>
        /// <example>public class Person { public string Name { get; set; } public int Age { get; set; } }</example>
        public string? GeneratedCode { get; set; }

        /// <summary>
        /// Error message (if conversion failed)
        /// </summary>
        /// <example>Invalid JSON format</example>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The target language used for conversion
        /// </summary>
        /// <example>CSharp</example>
        public string Language { get; set; } = string.Empty;
    }
} 