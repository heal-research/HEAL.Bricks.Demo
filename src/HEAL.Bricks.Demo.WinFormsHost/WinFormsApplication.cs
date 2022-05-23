#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;
using HEAL.Bricks.UI.WindowsForms;

namespace HEAL.Bricks.Demo.WinFormsHost {
  class WinFormsApplication : WindowsFormsApplication {
    public override string Name => "HEAL.Bricks.Demo.WinFormsApplication";
    public override string Description => "Simple WinForms GUI application.";

    public WinFormsApplication() : base() { }
    public WinFormsApplication(IChannel channel) : base(channel) { }

    public override Form CreateMainForm() {
      return new ApplicationForm {
        Text = "HEAL.Bricks.Demo.WinFormsApplication"
      };
    }
  }
}
