#region License Information
/*
 * This file is part of HEAL.Bricks.Demo which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks.Demo.StringFormatter.BasicFormatters {
  public class LowercaseFormatter : IStringFormatter {
    public string Format(string input) {
      return input.ToLower();
    }
  }
}
