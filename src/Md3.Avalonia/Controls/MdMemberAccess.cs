using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Md3.Avalonia.Controls;

/// <summary>A cached read/write accessor for one public instance property of a model type.</summary>
internal sealed class MdMemberAccessor
{
    private readonly PropertyInfo _property;

    internal MdMemberAccessor(PropertyInfo property) => _property = property;

    /// <summary>The declared type of the property, unwrapped from <see cref="Nullable{T}"/>.</summary>
    internal Type MemberType => Nullable.GetUnderlyingType(_property.PropertyType) ?? _property.PropertyType;

    /// <summary>Whether the property has an accessible setter.</summary>
    internal bool CanWrite => _property.CanWrite;

    internal object? Get(object item) => _property.GetValue(item);

    internal void Set(object item, object? value) => _property.SetValue(item, value);
}

/// <summary>
/// The single place in the library that resolves a string property path against a model type.
/// </summary>
/// <remarks>
/// <para>
/// Every control that accepts a path — <c>PropertyName</c>, <c>DisplayMemberPath</c>,
/// <c>ResultDisplayMemberPath</c> — funnels through here, so there is exactly one reflective
/// hole to reason about when the application is trimmed rather than one per control.
/// </para>
/// <para>
/// A trimmer cannot see through <c>item.GetType()</c>: the model type is the application's, not
/// the library's, so only the application can keep its properties. Each of those controls
/// therefore also exposes a delegate — <c>ValueSelector</c>, <c>DisplaySelector</c>,
/// <c>ResultDisplaySelector</c> — which reaches the same data with no reflection at all and is
/// the path to use in a trimmed app. See the trimming section of <c>docs/API.md</c>.
/// </para>
/// </remarks>
internal static class MdMemberAccess
{
    private static readonly ConcurrentDictionary<(Type Type, string Name), MdMemberAccessor?> Cache = new();

    /// <summary>
    /// Resolves <paramref name="name"/> on <paramref name="type"/>, or null when there is no such
    /// public instance property. Results are cached, so a sort or filter over thousands of rows
    /// pays for the lookup once rather than once per cell.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2070",
        Justification = "The model type belongs to the application, which is the only party that " +
                        "can preserve its properties. Controls that accept a string path also " +
                        "accept a reflection-free selector delegate; see the type remarks.")]
    internal static MdMemberAccessor? For(Type type, string name) =>
        Cache.GetOrAdd((type, name), static key =>
        {
            var property = key.Type.GetProperty(key.Name, BindingFlags.Instance | BindingFlags.Public);
            return property is null ? null : new MdMemberAccessor(property);
        });

    /// <summary>Reads <paramref name="path"/> off <paramref name="item"/>, or null when either is absent.</summary>
    internal static object? GetValue(object? item, string? path)
    {
        if (item is null || string.IsNullOrWhiteSpace(path)) return null;
        return For(item.GetType(), path)?.Get(item);
    }

    /// <summary>
    /// Reads <paramref name="path"/> as display text. An empty path means "use the item itself",
    /// which is what every control here does when no path is configured.
    /// </summary>
    internal static string GetText(object? item, string? path)
    {
        if (item is null) return string.Empty;
        if (string.IsNullOrWhiteSpace(path)) return item.ToString() ?? string.Empty;
        return GetValue(item, path)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Converts edited text to <paramref name="targetType"/>. Strings pass straight through;
    /// anything else goes through the type's converter.
    /// </summary>
    /// <returns>False when the text is not valid for the target type, leaving the model untouched.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "TypeDescriptor.GetConverter reads the application's model type. A trimmed " +
                        "app should supply MdDataGridColumn.ValueParser, which converts without " +
                        "reflection; see the type remarks.")]
    internal static bool TryConvert(string? text, Type targetType, out object? value)
    {
        if (targetType == typeof(string))
        {
            value = text;
            return true;
        }

        try
        {
            value = TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(text ?? string.Empty);
            return true;
        }
        catch
        {
            // A model's type converter is the application's code. A cell edit that cannot be
            // parsed is an ordinary outcome, not a reason to bring the window down.
            value = null;
            return false;
        }
    }
}
