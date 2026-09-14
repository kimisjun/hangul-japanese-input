namespace HangulJapaneseInput.App;

internal static class DiagnosticLog
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HangulJapaneseInput");
    internal static readonly string FilePath = Path.Combine(DirectoryPath, "diagnostic.log");

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.AppendAllText(FilePath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch { }
    }
}