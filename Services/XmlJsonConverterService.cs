using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace JsonConverterTool.Services
{
    public class XmlJsonConverterService
    {
        public string ConvertXmlToJson(string xmlContent)
        {
            try
            {
                // Load XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                // Convert to JSON
                string jsonText = JsonConvert.SerializeXmlNode(doc, Formatting.Indented, true);
                
                return jsonText;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting XML to JSON: {ex.Message}", ex);
            }
        }

        public string ConvertJsonToXml(string jsonContent)
        {
            try
            {
                // Parse JSON
                dynamic jsonObj = JsonConvert.DeserializeObject(jsonContent);
                
                // Create XML document
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xmlDeclaration);

                // Create root element
                XmlElement root = doc.CreateElement("root");
                doc.AppendChild(root);

                // Convert JSON to XML
                ConvertJsonObjectToXml(jsonObj, root, doc);

                // Format XML
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = new XmlTextWriter(stringWriter)
                {
                    Formatting = System.Xml.Formatting.Indented
                })
                {
                    doc.WriteTo(xmlTextWriter);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting JSON to XML: {ex.Message}", ex);
            }
        }

        private void ConvertJsonObjectToXml(dynamic jsonObj, XmlElement parent, XmlDocument doc)
        {
            if (jsonObj is Newtonsoft.Json.Linq.JObject jObject)
            {
                foreach (var property in jObject.Properties())
                {
                    string propertyName = property.Name;
                    dynamic propertyValue = property.Value;

                    if (propertyValue is Newtonsoft.Json.Linq.JObject)
                    {
                        XmlElement childElement = doc.CreateElement(propertyName);
                        parent.AppendChild(childElement);
                        ConvertJsonObjectToXml(propertyValue, childElement, doc);
                    }
                    else if (propertyValue is Newtonsoft.Json.Linq.JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            XmlElement arrayElement = doc.CreateElement(propertyName);
                            parent.AppendChild(arrayElement);

                            if (item is Newtonsoft.Json.Linq.JObject)
                            {
                                ConvertJsonObjectToXml(item, arrayElement, doc);
                            }
                            else
                            {
                                arrayElement.InnerText = item.ToString();
                            }
                        }
                    }
                    else
                    {
                        XmlElement element = doc.CreateElement(propertyName);
                        element.InnerText = propertyValue?.ToString() ?? string.Empty;
                        parent.AppendChild(element);
                    }
                }
            }
        }
    }
} 