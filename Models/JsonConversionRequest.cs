using System.ComponentModel.DataAnnotations;

namespace JsonConverterTool.Models
{
    /// <summary>
    /// Request model for JSON to code conversion
    /// </summary>
    public class JsonConversionRequest
    {
        /// <summary>
        /// The content to convert (JSON, XML, or YAML)
        /// </summary>
        /// <example>{ "name": "John", "age": 30 }</example>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The source format of the content
        /// </summary>
        /// <example>JSON</example>
        [Required]
        public string SourceFormat { get; set; } = "JSON";

        /// <summary>
        /// The target format or language (CSharp, Java, XML, YAML, JSON)
        /// </summary>
        /// <example>CSharp</example>
        [Required]
        public string TargetFormat { get; set; } = "CSharp";

        /// <summary>
        /// The name of the root class to generate (for code generation)
        /// </summary>
        /// <example>RootObject</example>
        public string RootClassName { get; set; } = "RootObject";

        /// <summary>
        /// Whether to generate properties (getters/setters) for fields (for code generation)
        /// </summary>
        /// <example>true</example>
        public bool GenerateProperties { get; set; } = true;

        /// <summary>
        /// Whether to generate JSON serialization attributes (for code generation)
        /// </summary>
        /// <example>true</example>
        public bool GenerateJsonAttributes { get; set; } = true;

        /// <summary>
        /// Whether to use PascalCase for property names (for code generation)
        /// </summary>
        /// <example>true</example>
        public bool UsePascalCase { get; set; } = true;
    }
} 