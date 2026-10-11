using System.Text.RegularExpressions;

namespace Md3.Avalonia.Chat;

/// <summary>Kind of a highlighted code token. The colour of each kind comes from the theme.</summary>
public enum MdCodeTokenKind
{
    /// <summary>Text without special meaning.</summary>
    Plain,
    /// <summary>A language keyword.</summary>
    Keyword,
    /// <summary>A type-like identifier, usually capitalised.</summary>
    Type,
    /// <summary>A string literal.</summary>
    String,
    /// <summary>A numeric literal.</summary>
    Number,
    /// <summary>A line comment.</summary>
    Comment,
}

/// <summary>One run of code text with its kind. Concatenating <see cref="Text"/> reproduces the code.</summary>
public sealed record MdCodeToken(string Text, MdCodeTokenKind Kind = MdCodeTokenKind.Plain);

/// <summary>
/// Splits code into coloured tokens. Applications supply an implementation for their languages;
/// <see cref="MdKeywordSyntaxHighlighter"/> is a small generic reference.
/// </summary>
public interface IMdSyntaxHighlighter
{
    /// <summary>Tokenises <paramref name="code"/>. The tokens must reproduce the code exactly.</summary>
    /// <param name="code">The code text.</param>
    /// <param name="language">The fence language label, or null when none was given.</param>
    IReadOnlyList<MdCodeToken> Highlight(string code, string? language);
}

/// <summary>
/// Reference highlighter for C-like and script-like code. It recognises line comments (// and #),
/// quoted strings, numbers, a common keyword list and capitalised type names. It ignores the language
/// label, so it is an approximation rather than a parser.
/// </summary>
public sealed class MdKeywordSyntaxHighlighter : IMdSyntaxHighlighter
{
    private static readonly Regex TokenPattern = new(
        @"(?<comment>//[^\n]*|#[^\n]*)|(?<string>""(?:\\.|[^""\\\n])*""|'(?:\\.|[^'\\\n])*')|(?<number>\b\d+(?:\.\d+)?\b)|(?<word>\b[A-Za-z_]\w*\b)",
        RegexOptions.CultureInvariant);

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "and", "as", "async", "await", "break", "case", "catch", "class", "const", "continue",
        "def", "default", "do", "elif", "else", "enum", "export", "false", "finally", "for", "foreach",
        "from", "fn", "func", "function", "if", "impl", "import", "in", "interface", "is", "lambda", "let",
        "namespace", "new", "not", "null", "or", "package", "pass", "private", "protected", "pub", "public",
        "return", "self", "static", "struct", "switch", "this", "throw", "trait", "true", "try", "using",
        "var", "void", "while", "with", "yield", "None", "True", "False",
    };

    /// <inheritdoc />
    public IReadOnlyList<MdCodeToken> Highlight(string code, string? language)
    {
        var tokens = new List<MdCodeToken>();
        var last = 0;
        foreach (Match match in TokenPattern.Matches(code))
        {
            if (match.Index > last) tokens.Add(new MdCodeToken(code[last..match.Index]));
            tokens.Add(new MdCodeToken(match.Value, KindOf(match)));
            last = match.Index + match.Length;
        }
        if (last < code.Length) tokens.Add(new MdCodeToken(code[last..]));
        return tokens;
    }

    private static MdCodeTokenKind KindOf(Match match)
    {
        if (match.Groups["comment"].Success) return MdCodeTokenKind.Comment;
        if (match.Groups["string"].Success) return MdCodeTokenKind.String;
        if (match.Groups["number"].Success) return MdCodeTokenKind.Number;
        if (!match.Groups["word"].Success) return MdCodeTokenKind.Plain;
        if (Keywords.Contains(match.Value)) return MdCodeTokenKind.Keyword;
        return char.IsUpper(match.Value[0]) ? MdCodeTokenKind.Type : MdCodeTokenKind.Plain;
    }
}
