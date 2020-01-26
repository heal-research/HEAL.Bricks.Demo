using System;

namespace HEAL.StringFormatter.BasicFormatters {
  public class UppercaseFormatter : IStringFormatter {
    public string Format(string input) {
      return input.ToUpper();
    }
  }
}
