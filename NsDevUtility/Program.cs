using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NsDevUtility
{
  class Program
  {
    static int Main(string[] args)
    {
      if (args == null || args.Length < 2)
        return Usage();

      var assemblyFilter = args[0];
      var outputFileName = args[1];

      if (args.Length > 2 && args[2] != null && args[2].Equals("-q"))
      {
        QuiteDown = true;
      }

      if (string.IsNullOrWhiteSpace(assemblyFilter) || string.IsNullOrWhiteSpace(outputFileName))
        return Usage();

      string[] files;

      if (File.Exists(assemblyFilter))
      {
        files = new[] { assemblyFilter };
      }
      else
      {
        var path = Path.GetDirectoryName(assemblyFilter);
        var filter = Path.GetFileName(assemblyFilter);
        files = Directory.GetFiles(path, filter);
      }

      if (files.Length == 0)
        return NoFilesFound();

      using (var fileStream = new StreamWriter(File.OpenWrite(outputFileName)))
      {
        fileStream.WriteLine("document ObjectTypes:");
        fileStream.WriteLine("  objectTypes:");
        foreach (var file in files)
        {
          var assembly = Assembly.LoadFrom(file);
          var viewModelTypes = GetViewModels(assembly);
          string assemblyName = assembly.GetName().Name;

          WriteLine("Assembly: {0}\tViewModels Found: {1}", FormatAssemblyName(assemblyName), viewModelTypes.Length);

          if (viewModelTypes.Length > 0)
          {
            WriteComment(fileStream, assemblyName);
            OutputToFile(fileStream, viewModelTypes);
          }
        }
      }

      return 0;
    }

    public static bool QuiteDown { get; private set; }

    private static string FormatAssemblyName(string assemblyName)
    {
      const int pad = 30;
      return assemblyName.Length > pad ? assemblyName.Substring(0, pad) + "..." : assemblyName.PadRight(pad);
    }

    private static Type[] GetViewModels(Assembly assembly)
    {
      try
      {
        return assembly.GetTypes().Where(x => x.BaseType != null && BaseTypeNames.Contains(x.BaseType.Name)).ToArray();
      }
      catch (ReflectionTypeLoadException ex)
      {

      }
      catch (TypeLoadException ex)
      {
      }
      return new Type[0];
    }

    public static string[] BaseTypeNames = new[] { "ItemViewModel`1", "ViewModel", "ListViewModel`1" };

    private static void OutputToFile(StreamWriter fileStream, IEnumerable<Type> types)
    {
      foreach (var type in types)
      {
        fileStream.WriteLine("    objectType {0}(type=\"{1}, {2}\")", type.Name, type.FullName, type.Assembly.GetName().Name);
      }
    }

    private static void WriteComment(StreamWriter fileStream, string assemblyName)
    {
      fileStream.WriteLine();
      fileStream.WriteLine();
      fileStream.WriteLine("    #==========================================================#");
      fileStream.WriteLine("    #{0,45}#", assemblyName);
      fileStream.WriteLine("    #==========================================================#");
    }

    private static int NoFilesFound()
    {
      WriteLine("No files found!");
      return 0;
    }

    private static int Usage()
    {
      var tmp = QuiteDown;
      QuiteDown = false;
      WriteLine("Usage: {0} assemblyFilter outputfile.ns", ProgramName);
      WriteLine("Example: {0} c:\\web\\bin\\Jet*.dll c:\\web\\configuration\\objectType.ns", ProgramName);
      QuiteDown = tmp;
      return 0;
    }

    private static void WriteLine(string message, params object[] args)
    {
      if (!QuiteDown)
      {
        Console.WriteLine(message, args);
      }
    }

    public static string ProgramName
    {
      get { return Process.GetCurrentProcess().ProcessName; }
    }
  }
}
