# DayCounter

A deliberately simple .NET MAUI app: the number of calendar days since your last alcoholic drink, and a reset-to-today button. Starts at zero on first use. Reset asks for confirmation. The date stays on the device across app restarts.

## Android home-screen widget

Touch and hold an empty area on the home screen, select Widgets, then add DayCounter. The widget shows the same count and opens the app when tapped. Resetting in the app updates all placed widgets immediately. Android periodically refreshes it (requested every 30 minutes) and it responds to date/time/time-zone changes and device restart. Background updates can be delayed by Android battery management; this is not an exact-midnight alarm.

Days are local calendar days, not completed 24-hour periods. Today is day 0; tomorrow is day 1. Clock changes cannot produce a negative count. There are no event lists, accounts, notifications or network features. Windows runs the counter app; the home-screen widget is Android only.

## Build and run

Open DayCounter.slnx in Visual Studio with .NET 11 and MAUI/Android workloads. Select Windows Machine or an Android device/emulator.

```powershell
dotnet build DayCounter.csproj -f net11.0-android
dotnet build DayCounter.csproj -f net11.0-windows10.0.19041.0
dotnet run --project tests/DayCounter.Checks.csproj
```

The saved date is `start-date.txt` in the app's private data directory. Uninstalling or clearing app data removes it. Android cloud backup is disabled.
