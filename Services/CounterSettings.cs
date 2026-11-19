namespace DayCounter.Services;

/// <summary>Settings shared by the MAUI page and Android widget without a database.</summary>
public sealed class CounterSettings(string directory)
{
    public const string Weather = "weather";
    public const string Classic = "classic";
    public const string Coastal = "coastal";

    public static int CoastalStage(int days) => Math.Clamp(days, 0, 11);

    private string PathFor(string name) => System.IO.Path.Combine(directory, name);

    public bool AllowDayChange
    {
        get => Read("allow-day-change.txt", "false") == "true";
        set => Write("allow-day-change.txt", value ? "true" : "false");
    }

    public string Theme
    {
        get => NormalizeTheme(Read("theme.txt", Weather));
        set => Write("theme.txt", NormalizeTheme(value));
    }

    private static string NormalizeTheme(string value) =>
        value is Classic or Coastal ? value : Weather;

    private string Read(string name, string fallback)
    {
        try
        {
            var path = PathFor(name);
            return File.Exists(path) ? File.ReadAllText(path).Trim() : fallback;
        }
        catch (IOException) { return fallback; }
        catch (UnauthorizedAccessException) { return fallback; }
    }

    private void Write(string name, string value)
    {
        Directory.CreateDirectory(directory);
        var path = PathFor(name);
        File.WriteAllText(path + ".tmp", value);
        File.Move(path + ".tmp", path, true);
    }
}
