using Microsoft.CodeAnalysis;

namespace SimpleGenerator
{

  [Generator]
  public class SimpleGenerator : ISourceGenerator
  {
    public void Initialize(GeneratorInitializationContext context)
    {
      // If you want to debug the actual source generator, enable the #if DEBUG part below
      // then when your IDE calls the generator, open a new copy of Visual Studio from the dialog
      // VS2022 should allow you to actually debug this in IDE, anything else will need the debug trap
      // #if DEBUG
      //       if (!Debugger.IsAttached)
      //       {
      //         Debugger.Launch();
      //       }
      // #endif 
    }

    public void Execute(GeneratorExecutionContext context)
    {
      // Find the main method
      var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

      // Build up the source code
      string source =
        $@"
// Auto-generated code
using System;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public partial class {mainMethod.ContainingType.Name}
    {{
        public static void GeneratedCall() =>
            Console.WriteLine($""Generator says: 'Well hello there' I'm in a method in a new partial class."");
    }}
}}
";

      var typeName = mainMethod.ContainingType.Name;

      // Add the source code to the compilation
      context.AddSource($"{typeName}.generated.cs", source);

    }

  }
}