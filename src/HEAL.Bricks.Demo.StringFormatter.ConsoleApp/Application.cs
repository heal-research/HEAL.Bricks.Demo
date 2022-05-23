#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks.Demo.StringFormatter.ConsoleApp {
  class App : Application {
    public override string Name => "HEAL.Bricks.Demo.StringFormatter";
    public override string Description => "Console application of the HEAL Bricks Demo String Formatter.";
    public override ApplicationKind Kind => ApplicationKind.Console;

    public override async Task RunAsync(string[] args, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
        IStringFormatter[] formatters = typeDiscoverer.GetInstances<IStringFormatter>().OrderBy(x => x.GetType().Name).ToArray();

        string input = ReadString();
        while (!string.IsNullOrEmpty(input)) {
          IStringFormatter? formatter = ChooseFormatter(formatters);
          Console.Write("output: ");
          Console.WriteLine(formatter?.Format(input) ?? "--- none ---");
          Console.WriteLine();
          input = ReadString();
        }
      }, cancellationToken);
    }

    private static string ReadString() {
      Console.Write("string > ");
      return Console.ReadLine() ?? string.Empty;
    }

    private static IStringFormatter? ChooseFormatter(IStringFormatter[] formatters) {
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
          _ = int.TryParse(Console.ReadLine(), out index);
        }
        return formatters[index - 1];
      }
    }
  }
}
