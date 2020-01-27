#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HEAL.StringFormatter {
  class Program {
    private static IPluginManager pluginManager;
    private static ITypeDiscoverer typeDiscoverer;
    static async Task Main() {
      Console.WriteLine("HEAL.Bricks.Demo");
      Console.WriteLine("================");
      Console.WriteLine();

      Console.Write("Initializing plugin manager ... ");
      pluginManager = PluginManager.Create("HEALStringFormatterPlugin", "https://api.nuget.org/v3/index.json", @"C:\# Daten\NuGet");
      await pluginManager.InitializeAsync();
      typeDiscoverer = TypeDiscoverer.Create();
      Console.WriteLine("done");
      Console.WriteLine();

      await MainMenu();
    }

    private static async Task MainMenu() {
      bool exit = false;
      while (!exit) {
        Console.WriteLine("Main Menu:");
        Console.WriteLine("----------");
        Console.WriteLine("[1] Show plugin manager status");
        Console.WriteLine("[2] List packages");
        Console.WriteLine("[3] List plugins");
        Console.WriteLine("[4] Resolve missing dependencies");
        Console.WriteLine("[5] Download missing dependencies");
        Console.WriteLine("[6] Load plugins");
        Console.WriteLine("[7] Start application");
        Console.WriteLine("[0] Exit");

        int actionIndex = ReadActionIndex("Action", 0, 7);
        Console.WriteLine();
        switch (actionIndex) {
          case 0: exit = true; break;
          case 1: ShowPluginManagerStatus(); break;
          case 2: ListPackages(); break;
          case 3: ListPlugins(); break;
          case 4: await ResolveMissingDependenciesAsync(); break;
          case 5: await DownloadMissingDependencies(); break;
          case 6: LoadPlugins(); break;
          case 7: StartApplication(); break;
        };
      }
    }

    private static void ShowPluginManagerStatus() {
      Console.WriteLine("Plugin Manager Status:");
      Console.WriteLine($"Status:              {pluginManager.Status}");
      Console.WriteLine($"Plugin tag:          {pluginManager.PluginTag}");
      Console.WriteLine($"Remote repositories: {string.Join(", ", pluginManager.RemoteRepositories)}");
      Console.WriteLine();
    }
    private static void ListPackages() {
      Console.WriteLine("Local Packages:");
      foreach (PackageInfo packageInfo in pluginManager.Packages) {
        Console.WriteLine(packageInfo.ToStringWithDependencies());
      }
      Console.WriteLine();
    }
    private static void ListPlugins() {
      Console.WriteLine("Local Plugins:");
      foreach (PackageInfo packageInfo in pluginManager.Plugins) {
        Console.WriteLine(packageInfo.ToStringWithDependencies());
      }
      Console.WriteLine();
    }
    private static async Task ResolveMissingDependenciesAsync() {
      Console.Write("Resolving missing dependencies ... ");
      await pluginManager.ResolveMissingDependenciesAsync();
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static async Task DownloadMissingDependencies() {
      Console.Write("Downloading missing dependencies ... ");
      await pluginManager.DownloadMissingDependenciesAsync();
      Console.WriteLine("done");
      Console.Write("Reinitializing plugin manager ... ");
      pluginManager = PluginManager.Create("HEALStringFormatterPlugin", "https://api.nuget.org/v3/index.json", @"C:\# Daten\NuGet");
      await pluginManager.InitializeAsync();
      typeDiscoverer = TypeDiscoverer.Create();
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static void LoadPlugins() {
      Console.WriteLine("Loading assemblies ...");
      NuGetAssemblyLoader.Load();
      Console.WriteLine("... done");
      Console.WriteLine();
    }
    private static void StartApplication() {
      Console.WriteLine("Applications:");
      IApplication[] applications = typeDiscoverer.GetInstances<IApplication>().OrderBy(x => x.Name).ToArray();
      for (int i = 0; i < applications.Length; i++) {
        Console.WriteLine($"[{i + 1}] {applications[i].Name}");
      }
      Console.WriteLine("[0] Back");

      int applicationIndex = ReadActionIndex("Application", 0, applications.Length);
      Console.WriteLine();
      if (applicationIndex == 0) return;
      else applications[applicationIndex - 1].Run(Array.Empty<ICommandLineArgument>());
      Console.WriteLine();
    }

    #region Helpers
    private static int ReadActionIndex(string prompt, int minIndex, int maxIndex) {
      int selectedIndex;
      do {
        Console.Write($"{prompt} > ");
        if (!int.TryParse(Console.ReadLine(), out selectedIndex)) {
          Console.WriteLine("Error: Not a valid number.");
          selectedIndex = -1;
        } else if ((selectedIndex < minIndex) || (selectedIndex > maxIndex)) {
          Console.WriteLine("Error: Selection is invalid.");
          selectedIndex = -1;
        }
      } while ((selectedIndex < minIndex) || (selectedIndex > maxIndex));
      return selectedIndex;
    }
    #endregion
  }
}
