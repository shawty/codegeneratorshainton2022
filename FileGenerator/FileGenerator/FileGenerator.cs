using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;

namespace FileGenerator
{

  [Generator]
  public class FileGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
      // If you want to debug the actual source generator, enable the #if DEBUG part below
      // then when your IDE calls the generator, open a new copy of Visual Studio from the dialog
      // VS2022 should allow you to actually debug this in IDE, anything else will need the debug trap
      #if DEBUG
             if (!Debugger.IsAttached)
             {
               Debugger.Launch();
             }
      #endif 
    }

    public void Execute(GeneratorExecutionContext context)
    {
      // Create a list of additional XML files added to the target project
      var additionalXmlFiles = context
        .AdditionalFiles
        .Where(name => name.Path.EndsWith(".xml"));

      foreach (var additionalXmlFile in additionalXmlFiles)
      {
        
        // Process our model definition here
        StreamReader reader = new StreamReader(additionalXmlFile.Path);
        XmlSerializer serializer = new XmlSerializer(typeof(PropertyDescriptionFile));

        PropertyDescriptionFile modelProperties = (PropertyDescriptionFile)serializer.Deserialize(reader);
    
        reader.Close();

        // Create the class name based on our file name
        string className = Path.ChangeExtension(Path.GetFileName(additionalXmlFile.Path), "").Trim('.');

        // Build out the string containing our new source code
        StringBuilder sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("using System.Xml.Serialization;");
        sourceBuilder.AppendLine("using System.Text.Json;");
        sourceBuilder.AppendLine("using System.Text.Json.Serialization;");

        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine($"[XmlRoot(ElementName = \"{className}\")]");
        sourceBuilder.AppendLine($"public class {className}");
        sourceBuilder.AppendLine("{");

        foreach (var modelProperty in modelProperties.Properties)
        {
          sourceBuilder.AppendLine($"  [XmlElement(ElementName = \"{modelProperty.XmlElementName}\")]");
          sourceBuilder.AppendLine($"  [JsonPropertyName(\"{modelProperty.JsonElementName}\")]");
          sourceBuilder.AppendLine($"  public {modelProperty.PropertyType} {modelProperty.XmlElementName} {{ get; set; }}");
          sourceBuilder.AppendLine();
        }

        sourceBuilder.AppendLine( "  public void LoadXml(string fileName)");
        sourceBuilder.AppendLine( "  {");
        sourceBuilder.AppendLine( "    StreamReader reader = new StreamReader(fileName);");
        sourceBuilder.AppendLine($"    XmlSerializer serializer = new XmlSerializer(typeof({className}));");
        sourceBuilder.AppendLine($"    {className} data = ({className})serializer.Deserialize(reader);");
        sourceBuilder.AppendLine();
        foreach (var modelProperty in modelProperties.Properties)
        {
          sourceBuilder.AppendLine($"    this.{modelProperty.XmlElementName} = data.{modelProperty.XmlElementName};");
        }

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine( "  }");
        sourceBuilder.AppendLine();
        
        sourceBuilder.AppendLine("  public void SaveJson(string fileName)");
        sourceBuilder.AppendLine("  {");
        sourceBuilder.AppendLine($"    {className} data = new {className}();");
        sourceBuilder.AppendLine();

        foreach (var modelProperty in modelProperties.Properties)
        {
          sourceBuilder.AppendLine($"    data.{modelProperty.XmlElementName} = this.{modelProperty.XmlElementName};");
        }

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"    string jsonString = JsonSerializer.Serialize(data);");
        sourceBuilder.AppendLine($"    File.WriteAllText(fileName, jsonString);");
        
        sourceBuilder.AppendLine("  }");
        sourceBuilder.AppendLine();
        
        sourceBuilder.AppendLine("}");
        
        context.AddSource($"{className}.generated.cs", sourceBuilder.ToString());
        
      }
      
    }

  }
}