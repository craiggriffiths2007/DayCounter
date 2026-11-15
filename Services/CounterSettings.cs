namespace DayCounter.Services;

/// <summary>Settings shared by the MAUI page and Android widget without a database.</summary>
public sealed class CounterSettings(string directory)
{
    public const string Weather = "weather";
    public const string Classic = "classic";

    private string PathFor(string name) => System.IO.Path.Combine(directory, name);

    public bool AllowDayChange
    {
        get => Read("allow-day-change.txt", "false") == "true";
        set => Write("allow-day-change.txt", value ? "true" : "false");
    }

    public string Theme
    {
        get
        {
            var value = Read("theme.txt", Weather);
            return value == Classic ? Classic : Weather;
        }
        set => Write("theme.txt", value == Classic ? Classic : Weather);
    }

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
