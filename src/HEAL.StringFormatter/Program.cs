#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HEAL.StringFormatter {
  class Program {
    private static IPackageManager packageManager;
    static async Task Main(string[] args) {
      if (Runner.ParseArguments(args, out CommunicationMode communicationMode, out string inputConnection, out string outputConnection)) {
        await Runner.ReceiveAndExecuteRunnerAsync(communicationMode, inputConnection, outputConnection);
        return;
      }

      Console.WriteLine("HEAL.Bricks.Demo");
      Console.WriteLine("================");
      Console.WriteLine();

      Console.Write("Initializing package manager ... ");
      Settings settings = new Settings();
      settings.PackageTag = "HEALStringFormatterPlugin";
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      packageManager = PackageManager.Create(settings);
      Console.WriteLine("done");
      Console.WriteLine();

      await MainMenu();
    }

    private static async Task MainMenu() {
      bool exit = false;
      while (!exit) {
        Console.WriteLine("Main Menu:");
        Console.WriteLine("----------");
        Console.WriteLine("[ 1] Show package manager status");
        Console.WriteLine("[ 2] List installed packages");
        Console.WriteLine("[ 3] Search packages");
        Console.WriteLine("[ 4] Install package");
        Console.WriteLine("[ 5] Remove installed package");
        Console.WriteLine("[ 6] Get missing dependencies");
        Console.WriteLine("[ 7] Install missing dependencies");
        Console.WriteLine("[ 8] Install updates");
        Console.WriteLine("[ 9] Load packages");
        Console.WriteLine("[10] Start application");
        Console.WriteLine("[ 0] Exit");

        int actionIndex = ReadActionIndex("Action", 0, 10);
        Console.WriteLine();
        switch (actionIndex) {
          case 0: exit = true; break;
          case 1: ShowPackageManagerStatus(); break;
          case 2: ListInstalledPackages(); break;
          case 3: await SearchPackagesAsync(); break;
          case 4: await InstallPackageAsync(); break;
          case 5: RemoveInstalledPackage(); break;
          case 6: await GetMissingDependenciesAsync(); break;
          case 7: await InstallMissingDependencies(); break;
          case 8: await InstallUpdatesAsync(); break;
          case 9: LoadPackages(); break;
          case 10: await StartApplicationAsync(); break;
        };
      }
    }

    private static void ShowPackageManagerStatus() {
      Console.WriteLine("Package Manager Status:");
      Console.WriteLine($"Packages:      {packageManager.InstalledPackages.Count()}");
      Console.WriteLine($"Status:        {packageManager.Status}");
      Console.WriteLine($"Package tag:   {packageManager.Settings.PackageTag}");
      Console.WriteLine($"Repositories:  {string.Join(", ", packageManager.Settings.Repositories)}");
      Console.WriteLine($"App path:      {packageManager.Settings.AppPath}");
      Console.WriteLine($"Packages path: {packageManager.Settings.PackagesPath}");
      Console.WriteLine($"Cache path:    {packageManager.Settings.PackagesCachePath}");
      Console.WriteLine();
    }
    private static void ListInstalledPackages() {
      Console.WriteLine("Installed Packages:");
      if (packageManager.InstalledPackages.Count() == 0) {
        Console.WriteLine("none");
      } else {
        foreach (LocalPackageInfo packageInfo in packageManager.InstalledPackages) {
          Console.WriteLine(packageInfo.ToStringWithDependencies());
        }
      }
      Console.WriteLine();
    }
    private static async Task SearchPackagesAsync() {
      bool includePreReleases = ReadYesNo("Include pre-releases?", false);
      string searchString = ReadString("Search String");
      Console.WriteLine("Searching packages ... ");
      int skip = 0;
      int take = 10;
      bool continueSearch;
      do {
        IEnumerable<(string Repository, RemotePackageInfo Package)> packages = await packageManager.SearchRemotePackagesAsync(searchString, skip, take, includePreReleases);
        foreach (IGrouping<string, (string Repository, RemotePackageInfo Package)> group in packages.GroupBy(x => x.Repository)) {
          Console.WriteLine($"Repository {group.Key}:");
          foreach ((string Repository, RemotePackageInfo Package) in group) {
            Console.WriteLine($"  - {Package}");
          }
        }
        if (packages.Count() == 0) {
          continueSearch = false;
        } else {
          continueSearch = ReadYesNo("Continue?", true);
          skip += take;
        }
      } while (continueSearch);
      Console.WriteLine();
    }
    private static async Task InstallPackageAsync() {
      string packageId = ReadString("Package", "HEAL.StringFormatter.ConsoleApp");
      string version = ReadString("Version", "0.1.0-alpha.1");
      bool installMissingDependencies = ReadYesNo("Install missing dependencies?", true);
      RemotePackageInfo package = await packageManager.GetRemotePackageAsync(packageId, version);
      if (package == null) {
        Console.WriteLine("Error: Package not found.");
      } else {
        Console.Write("Installing package ... ");
        await packageManager.InstallRemotePackageAsync(package, installMissingDependencies);
        Console.WriteLine("done");
      }
      Console.WriteLine();
    }
    private static void RemoveInstalledPackage() {
      Console.WriteLine("Installed Packages:");
      LocalPackageInfo[] packages = packageManager.InstalledPackages.ToArray();
      for (int i = 0; i < packages.Length; i++) {
        Console.WriteLine($"[{i + 1}] {packages[i]}");
      }
      Console.WriteLine("[0] Back to main menu");
      Console.WriteLine();

      int actionIndex = ReadActionIndex("Remove package", 0, packages.Length);
      if (actionIndex != 0) {
        Console.Write("Removing installed package ... ");
        packageManager.RemoveInstalledPackage(packages[actionIndex - 1]);
        Console.WriteLine("done");
      }
      Console.WriteLine();
    }
    private static async Task GetMissingDependenciesAsync() {
      Console.Write("Getting missing dependencies ... ");
      IEnumerable<RemotePackageInfo> missingDependencies = await packageManager.GetMissingDependenciesAsync();
      Console.WriteLine("done");

      if (missingDependencies.Count() == 0) {
        Console.WriteLine("none");
      } else {
        foreach (RemotePackageInfo package in missingDependencies) {
          Console.WriteLine(package.ToString());
        }
      }
      Console.WriteLine();
    }
    private static async Task InstallMissingDependencies() {
      Console.Write("Installing missing dependencies ... ");
      await packageManager.InstallMissingDependenciesAsync();
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static async Task InstallUpdatesAsync() {
      bool installMissingDependencies = ReadYesNo("Install missing dependencies?", true);
      bool includePreReleases = ReadYesNo("Include pre-releases?", false);
      Console.Write("Installing updates ... ");
      await packageManager.InstallPackageUpdatesAsync(installMissingDependencies, includePreReleases);
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static void LoadPackages() {
      Console.Write("Loading assemblies ... ");
      PackageLoader.LoadPackageAssemblies(packageManager.GetPackageLoadInfos());
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static async Task StartApplicationAsync() {
      Console.WriteLine("Applications:");

      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(packageManager.GetPackageLoadInfos());
      ApplicationInfo[] applications = await discoverApplicationsRunner.GetApplicationsAsync();
      for (int i = 0; i < applications.Length; i++) {
        Console.WriteLine($"[{i + 1}] {applications[i].Name}");
      }
      Console.WriteLine("[0] Back");

      int applicationIndex = ReadActionIndex("Application", 0, applications.Length);
      Console.WriteLine();
      if (applicationIndex == 0) return;
      else {
        ApplicationRunner applicationRunner = new ApplicationRunner(packageManager.GetPackageLoadInfos(), applications[applicationIndex - 1]);
        await applicationRunner.RunAsync();
      }
      Console.WriteLine();
    }

    #region Helpers
    private static int ReadActionIndex(string prompt, int minIndex, int maxIndex) {
      int selectedIndex;
      do {
        Console.Write($"{prompt} [{minIndex}-{maxIndex}] > ");
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
    private static string ReadString(string prompt, string defaultValue = "") {
      Console.Write($"{prompt} {(string.IsNullOrWhiteSpace(defaultValue) ? "" : "[" + defaultValue + "]")} > ");
      string input = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(input)) return defaultValue;
      return input;
    }
    private static bool ReadYesNo(string prompt, bool defaultValue) {
      Console.Write($"{prompt} [{(defaultValue ? "Y/n" : "y/N")}] > ");
      string input = Console.ReadLine();
      if ((input == "y") || (input == "Y")) return true;
      if ((input == "n") || (input == "N")) return false;
      return defaultValue;
    }
    #endregion
  }
}
