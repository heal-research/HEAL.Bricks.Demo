#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;

namespace HEAL.Bricks.Demo.ConsoleHost {
  class Program {
    private static IApplicationManager applicationManager;
    private static readonly BricksOptions options = BricksOptions.Default;

    static async Task Main(string[] args) {
      using (IChannel? channel = ProcessChannel.CreateFromCLIArguments(args)) {
        if (channel != null) {
          await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel);
          return;
        }
      }

      Console.WriteLine("HEAL.Bricks.Demo");
      Console.WriteLine("================");
      Console.WriteLine();

      Console.Write("Initializing Application Manager ... ");
      options.DefaultIsolation = Isolation.AnonymousPipes;
      //options.Repositories.Add(new Repository(@"C:\00-Data\NuGet"));
      Directory.CreateDirectory(options.PackagesPath);
      Directory.CreateDirectory(options.PackagesCachePath);
      applicationManager = ApplicationManager.Create(options);
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
        Console.WriteLine("[10] Change isolation mode");
        Console.WriteLine("[11] Start runnable");
        Console.WriteLine("[ 0] Exit");

        int actionIndex = ReadActionIndex("Action", 0, 11);
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
          case 10: await ChangeIsolationModeAsync(); break;
          case 11: await StartRunnableAsync(); break;
        };
      }
    }

    private static void ShowPackageManagerStatus() {
      Console.WriteLine("Package Manager Status:");
      Console.WriteLine($"Packages:           {applicationManager.PackageManager.InstalledPackages.Count()}");
      Console.WriteLine($"Status:             {applicationManager.PackageManager.Status}");
      Console.WriteLine($"Repositories:       {string.Join(", ", applicationManager.PackageManager.Options.Repositories.Select(x => x.Source))}");
      Console.WriteLine($"App path:           {applicationManager.PackageManager.Options.AppPath}");
      Console.WriteLine($"Packages path:      {applicationManager.PackageManager.Options.PackagesPath}");
      Console.WriteLine($"Cache path:         {applicationManager.PackageManager.Options.PackagesCachePath}");
      Console.WriteLine($"Isolation:          {applicationManager.PackageManager.Options.DefaultIsolation}");
      Console.WriteLine($"Starter Assembly:   {applicationManager.PackageManager.Options.StarterAssembly}");
      Console.WriteLine($"Dotnet command:     {applicationManager.PackageManager.Options.DotnetCommand}");
      Console.WriteLine($"Docker command:     {applicationManager.PackageManager.Options.DockerCommand}");
      Console.WriteLine($"Docker image:       {applicationManager.PackageManager.Options.DefaultDockerImage}");
      Console.WriteLine($"Windows Containers: {applicationManager.PackageManager.Options.UseWindowsContainer}");
      Console.WriteLine();
    }
    private static void ListInstalledPackages() {
      Console.WriteLine("Installed Packages:");
      if (!applicationManager.PackageManager.InstalledPackages.Any()) {
        Console.WriteLine("none");
      } else {
        foreach (LocalPackageInfo packageInfo in applicationManager.PackageManager.InstalledPackages) {
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
        IEnumerable<(string Repository, RemotePackageInfo Package)> packages = await applicationManager.PackageManager.SearchRemotePackagesAsync(searchString, skip, take, includePreReleases);
        foreach (IGrouping<string, (string Repository, RemotePackageInfo Package)> group in packages.GroupBy(x => x.Repository)) {
          Console.WriteLine($"Repository {group.Key}:");
          foreach ((string Repository, RemotePackageInfo Package) in group) {
            Console.WriteLine($"  - {Package}");
          }
        }
        if (!packages.Any()) {
          continueSearch = false;
        } else {
          continueSearch = ReadYesNo("Continue?", true);
          skip += take;
        }
      } while (continueSearch);
      Console.WriteLine();
    }
    private static async Task InstallPackageAsync() {
      string packageId = ReadString("Package", "HEAL.Bricks.Demo.StringFormatter.ConsoleApp");
      string version = ReadString("Version", "0.4.1-alpha.1");
      bool installMissingDependencies = ReadYesNo("Install missing dependencies?", true);
      RemotePackageInfo? package = await applicationManager.PackageManager.GetRemotePackageAsync(packageId, version);
      if (package == null) {
        Console.WriteLine("Error: Package not found.");
      } else {
        Console.Write("Installing package ... ");
        await applicationManager.PackageManager.InstallRemotePackageAsync(package, installMissingDependencies);
        Console.WriteLine("done");
      }
      Console.WriteLine();
    }
    private static void RemoveInstalledPackage() {
      Console.WriteLine("Installed Packages:");
      LocalPackageInfo[] packages = applicationManager.PackageManager.InstalledPackages.ToArray();
      for (int i = 0; i < packages.Length; i++) {
        Console.WriteLine($"[{i + 1}] {packages[i]}");
      }
      Console.WriteLine("[0] Back to main menu");
      Console.WriteLine();

      int actionIndex = ReadActionIndex("Remove package", 0, packages.Length);
      if (actionIndex != 0) {
        Console.Write("Removing installed package ... ");
        applicationManager.PackageManager.RemoveInstalledPackage(packages[actionIndex - 1]);
        Console.WriteLine("done");
      }
      Console.WriteLine();
    }
    private static async Task GetMissingDependenciesAsync() {
      Console.Write("Getting missing dependencies ... ");
      IEnumerable<RemotePackageInfo> missingDependencies = await applicationManager.PackageManager.GetMissingDependenciesAsync();
      Console.WriteLine("done");

      if (!missingDependencies.Any()) {
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
      await applicationManager.PackageManager.InstallMissingDependenciesAsync();
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static async Task InstallUpdatesAsync() {
      bool installMissingDependencies = ReadYesNo("Install missing dependencies?", true);
      bool includePreReleases = ReadYesNo("Include pre-releases?", false);
      Console.Write("Installing updates ... ");
      await applicationManager.PackageManager.InstallPackageUpdatesAsync(installMissingDependencies, includePreReleases);
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static void LoadPackages() {
      Console.Write("Loading assemblies ... ");
      PackageLoader.LoadPackageAssemblies(applicationManager.PackageManager.GetPackageLoadInfos());
      Console.WriteLine("done");
      Console.WriteLine();
    }
    private static Task ChangeIsolationModeAsync() {
      Console.WriteLine("Isolation Modes:");
      string[] isolationModes = Enum.GetNames(typeof(Isolation));
      for (int i = 0; i < isolationModes.Length; i++) {
        Console.WriteLine($"[{i + 1}] {isolationModes[i]}");
      }
      Console.WriteLine("[0] Back to main menu");
      Console.WriteLine();

      int actionIndex = ReadActionIndex("Isolation mode", 0, isolationModes.Length);
      if (actionIndex != 0) {
        Console.Write("Setting isolation mode ... ");
        options.DefaultIsolation = Enum.Parse<Isolation>(isolationModes[actionIndex - 1]);
        applicationManager = ApplicationManager.Create(options);
        Console.WriteLine("done");
      }
      Console.WriteLine();
      return Task.CompletedTask;
    }

    private static async Task StartRunnableAsync() {
      Console.WriteLine("Runnables:");

      applicationManager = ApplicationManager.Create(options);
      int i = 1;
      foreach (RunnableInfo runnable in applicationManager.InstalledRunnables) {
        Console.WriteLine($"[{i}] {runnable.Name}");
        i++;
      }
      Console.WriteLine("[0] Back");

      int runnableIndex = ReadActionIndex("Runnable", 0, applicationManager.InstalledRunnables.Count());
      Console.WriteLine();
      if (runnableIndex == 0) return;
      else {
        await applicationManager.RunAsync(applicationManager.InstalledRunnables.ElementAt(runnableIndex - 1));
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
      string input = Console.ReadLine() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(input)) return defaultValue;
      return input;
    }
    private static bool ReadYesNo(string prompt, bool defaultValue) {
      Console.Write($"{prompt} [{(defaultValue ? "Y/n" : "y/N")}] > ");
      string input = Console.ReadLine() ?? string.Empty;
      if ((input == "y") || (input == "Y")) return true;
      if ((input == "n") || (input == "N")) return false;
      return defaultValue;
    }
    #endregion
  }
}
