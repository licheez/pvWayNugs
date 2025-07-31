using System.ComponentModel;

namespace pvNugsEnumConvNc9;

/// <summary>
/// Provides utility methods for converting between enum values and their string representations,
/// utilizing the Description attribute for custom string mappings.
/// </summary>
public static class EnumConvert
{
    /// <summary>
    /// Gets the code associated with an enum value from its Description attribute.
    /// If the Description contains multiple values separated by commas, returns the first value.
    /// </summary>
    /// <param name="value">The enum value to get the code for.</param>
    /// <returns>The code string from the Description attribute, or the first code if multiple are defined.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the enum value doesn't have a Description attribute.</exception>
    public static string GetCode(this Enum value)
    {
        var rawDescription = GetRawDescription(value);
        if (!rawDescription.Contains(',')) return rawDescription;
        var codeList = rawDescription.Split(',');
        return codeList[0];
    }
    
    /// <summary>
    /// Gets the raw description string from an enum value's Description attribute.
    /// </summary>
    /// <param name="value">The enum value to get the description for.</param>
    /// <returns>The complete Description attribute string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the enum value doesn't have a Description attribute.</exception>
    private static string GetRawDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString())!;
        var descriptionAttributes = fieldInfo.GetCustomAttributes(
            typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            
        if (descriptionAttributes != null && descriptionAttributes.Length > 0)
            return descriptionAttributes[0].Description;
        
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts a string code to its corresponding enum value using a custom matching function.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="code">The code string to convert.</param>
    /// <param name="match">A function that defines how to match the code with enum descriptions.</param>
    /// <returns>The matching enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when code is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when no matching enum value is found.</exception>
    public static T GetValue<T>(string code, Func<string, string, bool> match) where T : Enum
    {
        if (string.IsNullOrEmpty(code)) 
            throw new ArgumentNullException(nameof(code));
            
        var found = TryFindCode<T>(code, match, out var value);
        if (found) return value!;
        
        throw new ArgumentOutOfRangeException(nameof(code), code, null);
    }
    
    /// <summary>
    /// Converts a string code to its corresponding enum value, returning a default value if no match is found.
    /// Uses case-insensitive comparison by default.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="code">The code string to convert.</param>
    /// <param name="defaultValue">The default value to return if no match is found.</param>
    /// <returns>The matching enum value or the default value if no match is found.</returns>
    public static T GetValue<T>(string? code, T defaultValue) where T : Enum =>
        GetValue(code, defaultValue, Matcher);

    /// <summary>
    /// Converts a string code to its corresponding enum value using a custom matching function,
    /// returning a default value if no match is found.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="code">The code string to convert.</param>
    /// <param name="defaultValue">The default value to return if no match is found.</param>
    /// <param name="match">A function that defines how to match the code with enum descriptions.</param>
    /// <returns>The matching enum value or the default value if no match is found.</returns>
    public static T GetValue<T>(string? code, T defaultValue, Func<string, string, bool> match) where T : Enum
    {
        if (string.IsNullOrEmpty(code)) return defaultValue;
        
        var found = TryFindCode<T>(code, match, out var value);
        
        return found ? value! : defaultValue;
    }

    /// <summary>
    /// Attempts to find an enum value matching the provided code using a custom matching function.
    /// </summary>
    /// <typeparam name="T">The enum type to search within.</typeparam>
    /// <param name="code">The code to search for.</param>
    /// <param name="match">The function to use for matching codes.</param>
    /// <param name="value">The matching enum value if found, default if not found.</param>
    /// <returns>True if a matching enum value was found, false otherwise.</returns>
    private static bool TryFindCode<T>(string code, Func<string, string, bool> match, out T? value) where T : Enum
    {
        value = default;
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        
        foreach (var enumValue in values)
        {
            var rawDescription = enumValue.GetRawDescription();
            var vCodes = rawDescription.Split(',',
                    StringSplitOptions.TrimEntries)
                .ToList();
                
            if (!vCodes.Exists(vCode => match(code, vCode))) continue;
            value = enumValue;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Default matching function that performs case-insensitive string comparison.
    /// </summary>
    /// <param name="x">First string to compare.</param>
    /// <param name="y">Second string to compare.</param>
    /// <returns>True if the strings match (ignoring case), false otherwise.</returns>
    private static bool Matcher(string x, string y) =>
        x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
}