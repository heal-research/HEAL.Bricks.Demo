using System;

namespace HEAL.StringFormatter.BasicFormatters {
  public class LowercaseFormatter : IStringFormatter {
    public string Format(string input) {
      return input.ToLower();
    }
  }
}
