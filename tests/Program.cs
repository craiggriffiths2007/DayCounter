using DayCounter.Models;
using DayCounter.Services;
static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS " + label); }
var today = new DateTime(2026, 11, 15);
Check(DayCount.Since(today, today) == 0, "Reset day is zero");
Check(DayCount.Since(today.AddDays(-10), today) == 10, "Elapsed days");
Check(DayCount.Since(today.AddMinutes(-1), today) == 1, "Local midnight boundary");
Check(DayCount.Since(today, today.AddHours(23)) == 0, "Same calendar date");
Check(DayCount.Since(today.AddDays(1), today) == 0, "Clock rollback stays nonnegative");
Check(DayCount.Since(new(2024,2,28), new(2024,3,1)) == 2, "Leap year");
var dir = Path.Combine(Path.GetTempPath(), "DayCounterChecks-" + Guid.NewGuid());
var path = Path.Combine(dir, "start-date.txt");
try {
 var store = new StartDateStore(path);
 Check(store.GetOrCreate(today) == today, "First use starts today");
 Check(new StartDateStore(path).GetOrCreate(today.AddDays(5)) == today, "Persists across restart");
 store.Reset(today.AddDays(5));
 Check(store.GetOrCreate(today.AddDays(5)) == today.AddDays(5), "Reset persists");
 File.WriteAllText(path, "broken");
 bool rejected = false;
 try { store.GetOrCreate(today); } catch (FormatException) { rejected = true; }
 Check(rejected && File.ReadAllText(path) == "broken", "Unreadable date preserved");
} finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
