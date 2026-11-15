using System.Globalization;

namespace DayCounter.Services;

public sealed class StartDateStore(string path)
{
    public DateTime GetOrCreate(DateTime today)
    {
        if (!File.Exists(path)) { Reset(today); return today.Date; }
        return DateTime.ParseExact(File.ReadAllText(path).Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public void Reset(DateTime today)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path + ".tmp", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        File.Move(path + ".tmp", path, true);
    }
}
