namespace SimpleCrwaler.Common;

public static class StringExtentions
{
    public static bool HasValue(this string? value) => !string.IsNullOrWhiteSpace(value);

    public static string RemoveParameters(this string? url)
    {
        Uri uri = new Uri(url);
        string baseUrl = uri.GetLeftPart(UriPartial.Path);
        return baseUrl;
    }

    public static bool HasValue(this string? text,Action action)
    {
        bool condition = !string.IsNullOrWhiteSpace(text);
        if (condition)
        {
            action();
        }

        return condition;
    }

    public static bool HasValue(this string? text, Func<Exception> func)
    {
        bool condition = !string.IsNullOrWhiteSpace(text);
        if (!condition)
        {
            throw func();
        }

        return condition;
    }
}