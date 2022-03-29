using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;

namespace DataApiGenerator
{
  [Generator]
  public class DataApiGenerator : ISourceGenerator
  {
    private const string DefinitionFileName = "DATAMODELDEFINITION.XML";

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

    // This is the ACTUAL generator that creates the new source files
    public void Execute(GeneratorExecutionContext context)
    {
      // First off lets find out if our target project actually includes our definition file 
      var additionalXmlFiles = context
        .AdditionalFiles
        .Where(name => name.Path.EndsWith(".xml"))
        .ToDictionary(
          file => Path.GetFileName(file.Path).ToUpper(),
          file => file.Path
        );

      // If we have a definition file, then let's go ahead and read it in
      if (additionalXmlFiles.ContainsKey(DefinitionFileName))
      {
        // Process our model definition here
        StreamReader reader = new StreamReader(additionalXmlFiles[DefinitionFileName]);
        XmlSerializer serializer = new XmlSerializer(typeof(DataModel));

        DataModel modelData = (DataModel) serializer.Deserialize(reader);

        reader.Close();

        // Now we have our definition loaded, we can use the information contained in it, to generate our new code
        GenerateModelClasses(context, modelData);
        GenerateEfContext(context, modelData);
        GenerateServiceClasses(context, modelData);
        GenerateControllerClasses(context, modelData);
      }
    }

    private void GenerateModelClasses(GeneratorExecutionContext context, DataModel model)
    {
      foreach (NameSpace myNameSpace in model.NameSpace)
      {
        string root = myNameSpace.RootName;

        foreach (DataClass dataClass in myNameSpace.Classes)
        {
          StringBuilder sb = new StringBuilder();

          sb.AppendLine("using System;");
          sb.AppendLine("using System.ComponentModel.DataAnnotations;");
          sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
          sb.AppendLine();

          sb.AppendLine($"namespace {root}");
          sb.AppendLine("{");

          string className = dataClass.Name;

          sb.AppendLine($"  public class {className}");
          sb.AppendLine("  {");

          foreach (ClassProperty classProperty in dataClass.Properties)
          {
            if (classProperty.IsKey)
            {
              sb.AppendLine("[Key]");
            }

            sb.AppendLine($"    public {classProperty.Type} {classProperty.Name} {{ get; set; }}");
          }

          sb.AppendLine("  }");
          sb.AppendLine("}\n\n");

          context.AddSource($"{className}.generated.cs", sb.ToString());
        }
      }
    }

    private void GenerateEfContext(GeneratorExecutionContext context, DataModel model)
    {
      StringBuilder sb = new StringBuilder();

      foreach (NameSpace myNameSpace in model.NameSpace)
      {
        string rootName = myNameSpace.RootName;

        sb.AppendLine($"using {rootName};");
      }

      sb.AppendLine("using Microsoft.EntityFrameworkCore;");
      sb.AppendLine();

      sb.AppendLine($"namespace {model.DcNameSpace};");
      sb.AppendLine();

      sb.AppendLine($"public class {model.DcClassName} : DbContext");
      sb.AppendLine("{");

      sb.AppendLine($"  public {model.DcClassName}(DbContextOptions<{model.DcClassName}> options) : base(options)");
      sb.AppendLine("  {}");
      sb.AppendLine();

      foreach (var myNameSpace in model.NameSpace)
      {
        foreach (DataClass dataClass in myNameSpace.Classes)
        {
          sb.AppendLine($"  public DbSet<{dataClass.Name}>? {dataClass.Name} {{ get; set; }}");
        }
      }

      sb.AppendLine();


      sb.AppendLine("}");

      context.AddSource($"{model.DcClassName}.generated.cs", sb.ToString());
    }

    private void GenerateServiceClasses(GeneratorExecutionContext context, DataModel model)
    {
      foreach (NameSpace myNameSpace in model.NameSpace)
      {
        string dataModelRoot = myNameSpace.RootName;

        foreach (DataClass dataClass in myNameSpace.Classes)
        {
          StringBuilder sb = new StringBuilder();

          sb.AppendLine($"using {dataModelRoot};");
          sb.AppendLine();

          sb.AppendLine($"namespace {model.ServicesNameSpace}");
          sb.AppendLine("{");

          string className = dataClass.Name;

          sb.AppendLine($"  public class {className}Service");
          sb.AppendLine("  {");
          sb.AppendLine($"    private readonly {model.DcClassName} _dc;");

          sb.AppendLine($"    public {className}Service({model.DcClassName} dc)");
          sb.AppendLine("    {");
          sb.AppendLine($"      _dc = dc;");
          sb.AppendLine("    }");
          sb.AppendLine();

          sb.AppendLine($"    public {className} FetchSingle(int id)");
          sb.AppendLine("    {");
          sb.AppendLine($"      return _dc.{className}.FirstOrDefault(r => r.Pkid == id);");
          sb.AppendLine("    }");
          sb.AppendLine();
          sb.AppendLine("    // Add more methods here as needed");
          sb.AppendLine();
          sb.AppendLine("  }");

          sb.AppendLine("}\n\n");

          context.AddSource($"{className}Service.generated.cs", sb.ToString());
        }
      }
    }

    private void GenerateControllerClasses(GeneratorExecutionContext context, DataModel model)
    {
      foreach (NameSpace myNameSpace in model.NameSpace)
      {
        string dataModelRoot = myNameSpace.RootName;

        foreach (DataClass dataClass in myNameSpace.Classes)
        {
          StringBuilder sb = new StringBuilder();

          sb.AppendLine($"using {dataModelRoot};");
          sb.AppendLine($"using {model.ServicesNameSpace};");
          sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
          sb.AppendLine();

          sb.AppendLine($"namespace {model.ControllersNameSpace}");
          sb.AppendLine("{");

          string className = dataClass.Name;

          sb.AppendLine("  [Route(\"/api/[controller]\")]");
          sb.AppendLine($"  public class {className}Controller : Controller");
          sb.AppendLine("  {");
          sb.AppendLine($"    private readonly {className}Service _{className}Service;");

          sb.AppendLine($"    public {className}Controller({className}Service {className}Service)");
          sb.AppendLine("    {");
          sb.AppendLine($"      _{className}Service = {className}Service;");
          sb.AppendLine("    }");
          sb.AppendLine();

          sb.AppendLine("    [HttpGet(\"single/{id}\")]");
          sb.AppendLine($"    public IActionResult FetchSingle(int id)");
          sb.AppendLine("    {");
          sb.AppendLine($"      var result = _{className}Service.FetchSingle(id);");
          sb.AppendLine($"      return Ok(result);");
          sb.AppendLine("    }");
          sb.AppendLine();
          sb.AppendLine("    // Add more methods here as needed");
          sb.AppendLine();
          sb.AppendLine("  }");

          sb.AppendLine("}\n\n");

          context.AddSource($"{className}Controller.generated.cs", sb.ToString());
        }
      }
    }
  }
}