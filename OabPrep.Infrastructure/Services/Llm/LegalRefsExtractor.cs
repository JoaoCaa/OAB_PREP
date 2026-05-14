using System.Text.RegularExpressions;

namespace OabPrep.Infrastructure.Services.Llm;

internal static class LegalRefsExtractor
{
    private static readonly Regex Pattern = new(
        @"\b(?:" +
        @"art(?:igo)?\.?\s*\d+[°º]?(?:[,\s]+(?:§\s*\d+[°º]?|inc(?:iso)?\.?\s*[IVXivx]+))*" +
        @"|§\s*\d+[°º]?" +
        @"|Lei\s+n[°º\.]\s*\d+[\.,/]\d{2,4}" +
        @"|CF/88|CRFB/88|CRFB" +
        @"|CC/02|CC/2002" +
        @"|(?:CP|CDC|CLT|CPC|CPP|CTN|ECA|LINDB|STF|STJ)\b" +
        @")",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string[] Extract(string text) =>
        Pattern.Matches(text)
               .Select(m => m.Value.Trim())
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .ToArray();
}
