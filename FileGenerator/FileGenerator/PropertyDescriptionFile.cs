using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileGenerator
{
  [XmlRoot(ElementName = "properties")]
  public class PropertyDescriptionFile
  {
    [XmlElement(ElementName = "property")]
    
    public Property[] Properties { get; set; }
  }

  [XmlRoot(ElementName = "property")]
  public class Property
  {
    [XmlAttribute(AttributeName = "type")] 
    public string PropertyType { get; set; }

    [XmlAttribute(AttributeName = "xmlElementName")]
    public string XmlElementName { get; set; }

    [XmlAttribute(AttributeName = "jsonElementName")]
    public string JsonElementName { get; set; }
  }
  
}