using System.ComponentModel;
// ReSharper disable MemberCanBePrivate.Global

namespace pvNugsEnumConvNc9;

public static class EnumConvert
{
    public static string GetCode(this Enum value)
    {
        var rawDescription = GetRawDescription(value);
        if (!rawDescription.Contains(',')) return rawDescription;
        var codeList = rawDescription.Split(',');
        return codeList[0];
    }
    
    private static string GetRawDescription(this Enum value)
    {
        var fieldInfo = value
            .GetType()
            .GetField(value.ToString())!;
        DescriptionAttribute[]? descriptionAttributes = null;
        try
        {
            descriptionAttributes = (DescriptionAttribute[]?)fieldInfo
                .GetCustomAttributes(
                    typeof(DescriptionAttribute), false);
        }
        catch (Exception)
        {
            // nop
        }
        if (descriptionAttributes is not null 
            && descriptionAttributes.Length > 0)
            return descriptionAttributes[0].Description;
        
        throw new ArgumentOutOfRangeException(
            nameof(value), value, null);
    }

    public static T GetValue<T>(
        string code,
        Func<string, string, bool> match) where T : Enum
    {
        if (string.IsNullOrEmpty(code)) 
            throw new ArgumentNullException(nameof(code));
        var found = TryFindCode<T>(code, match, out var value);
        if (found) return value!;
        throw new ArgumentOutOfRangeException(
                nameof(code), code, null);
    }
    
    public static T GetValue<T>(
        string? code,
        T defaultValue) where T : Enum =>
        GetValue(code, defaultValue, Matcher);

    public static T GetValue<T>(
        string? code,
        T defaultValue,
        Func<string, string, bool> match) where T : Enum
    {
        if (string.IsNullOrEmpty(code)) return defaultValue;
        
        var found = TryFindCode<T>(code, match, out var value);
        
        return found ? value! : defaultValue;
    }

    private static bool TryFindCode<T>(
        string code,
        Func<string, string, bool> match,
        out T? value) where T : Enum
    {
        value = default;
        var values = 
            Enum.GetValues(typeof(T)).Cast<T>();
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
    
    private static bool Matcher(string x, string y) =>
        x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
    
}