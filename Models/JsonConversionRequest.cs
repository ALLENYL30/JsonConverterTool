using System.ComponentModel.DataAnnotations;

namespace JsonConverterTool.Models
{
    /// <summary>
    /// Request model for JSON to code conversion
    /// </summary>
    public class JsonConversionRequest
    {
        /// <summary>
        /// The JSON content to convert
        /// </summary>
        /// <example>{ "name": "John", "age": 30 }</example>
        [Required]
        public string JsonContent { get; set; } = string.Empty;

        /// <summary>
        /// The target programming language (CSharp or Java)
        /// </summary>
        /// <example>CSharp</example>
        [Required]
        public string TargetLanguage { get; set; } = "CSharp"; // CSharp or Java

        /// <summary>
        /// The name of the root class to generate
        /// </summary>
        /// <example>RootObject</example>
        [Required]
        public string RootClassName { get; set; } = "RootObject";

        /// <summary>
        /// Whether to generate properties (getters/setters) for fields
        /// </summary>
        /// <example>true</example>
        public bool GenerateProperties { get; set; } = true;

        /// <summary>
        /// Whether to generate JSON serialization attributes
        /// </summary>
        /// <example>true</example>
        public bool GenerateJsonAttributes { get; set; } = true;

        /// <summary>
        /// Whether to use PascalCase for property names
        /// </summary>
        /// <example>true</example>
        public bool UsePascalCase { get; set; } = true;
    }
} 