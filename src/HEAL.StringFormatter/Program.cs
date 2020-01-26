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
    static async Task Main() {
      IPluginManager pluginManager = PluginManager.Create("HEALStringFormatterPlugin", "https://api.nuget.org/v3/index.json");
      await pluginManager.InitializeAsync();
      NuGetAssemblyLoader.Load();
      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      IEnumerable<IApplication> apps = typeDiscoverer.GetInstances<IApplication>();
      apps.FirstOrDefault()?.Run(Array.Empty<ICommandLineArgument>());
    }
  }
}
