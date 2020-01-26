using HEAL.Bricks;
using System;
using System.Linq;

namespace HEAL.StringFormatter.ConsoleApp {
  class Application : IApplication {
    public string Name => "HEAL.StringFormatter.ConsoleApp";

    public string Description => "Console application of the HEAL String Formatter.";

    public void OnCancel() {
      throw new NotImplementedException();
    }

    public void OnPause() {
      throw new NotImplementedException();
    }

    public void OnResume() {
      throw new NotImplementedException();
    }

    public void Run(ICommandLineArgument[] args) {
      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      IStringFormatter[] formatters = typeDiscoverer.GetInstances<IStringFormatter>().OrderBy(x => x.GetType().Name).ToArray();

      string input = ReadString();
      while (!string.IsNullOrEmpty(input)) {
        IStringFormatter formatter = ChooseFormatter(formatters);
        Console.Write("output: ");
        Console.WriteLine(formatter?.Format(input) ?? "--- none ---");
        Console.WriteLine();
        input = ReadString();
      }
    }

    private string ReadString() {
      Console.Write("string > ");
      return Console.ReadLine();
    }

    private IStringFormatter ChooseFormatter(IStringFormatter[] formatters) {
      if (formatters.Length == 0) {
        Console.WriteLine("No formatters available.");
        return null;
      } else {
        Console.WriteLine("Available formatters:");
        for (int i = 0; i < formatters.Length; i++) {
          Console.WriteLine($"[{i + 1}] {formatters[i].GetType().Name}");
        }
        int index = -1;
        while (index < 1 || index > formatters.Length) {
          Console.Write("formatter > ");
          int.TryParse(Console.ReadLine(), out index);
        }
        return formatters[index - 1];
      }
    }
  }
}
