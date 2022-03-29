using System.Xml.Serialization;

// Simple standard class definition designed to represent the de-serialization of the XML
// model definition, this generator expects to find in the target project.
namespace DataApiGenerator
{
  
  [XmlRoot(ElementName = "Model")]
  public class DataModel
  {
    [XmlElement(ElementName = "NameSpace")]
    public NameSpace[] NameSpace { get; set; }
    
    [XmlElement(ElementName = "DcNameSpace")]
    public string DcNameSpace { get; set; }
    
    [XmlElement(ElementName = "DcClassName")]
    public string DcClassName { get; set; }
    
    [XmlElement(ElementName = "ServicesNameSpace")]
    public string ServicesNameSpace { get; set; }
    
    [XmlElement(ElementName = "ControllersNameSpace")]
    public string ControllersNameSpace { get; set; }
  }

  [XmlRoot(ElementName = "NameSpace")]
  public class NameSpace
  {
    [XmlAttribute(AttributeName = "Root")]
    public string RootName { get; set; }

    [XmlElement(ElementName = "Class")]
    public DataClass[] Classes { get; set; }
  }

  [XmlRoot(ElementName = "Class")]
  public class DataClass
  {
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }
    
    [XmlElement(ElementName = "Property")]
    public ClassProperty[] Properties { get; set; }
  }

  [XmlRoot(ElementName = "Property")]
  public class ClassProperty
  {
    [XmlAttribute(AttributeName = "Type")]
    public string Type { get; set; }
    
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }
    
    [XmlAttribute(AttributeName = "IsKey")]
    public bool IsKey { get; set; }
  }
  
}