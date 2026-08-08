namespace ELearningManagementSystem.Shared.Extensions;

public static class StringExtensions
{
    public static string ToSlug(this string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }
}
