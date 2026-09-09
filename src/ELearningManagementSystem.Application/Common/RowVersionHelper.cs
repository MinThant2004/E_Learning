namespace ELearningManagementSystem.Application.Common;

public static class RowVersionHelper
{
    public static byte[]? Decode(string? rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion)) return null;
        try
        {
            return Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}