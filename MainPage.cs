using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

public sealed class MainPage : ContentPage
{
    private readonly StartDateStore store = new(Path.Combine(FileSystem.AppDataDirectory, "start-date.txt"));
    private readonly Label count = new() { FontSize = 110, FontFamily = "OpenSansSemibold", TextColor = Color.FromArgb("#18796B"), HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label since = Text("", 15);
    private readonly Label unit = Text("DAYS SINCE LAST DRINK", 13);
    private IDispatcherTimer? timer;

    public MainPage()
    {
        Title = "DayCounter";
        BackgroundColor = Color.FromArgb("#F4F7F3");
        var reset = new Button { Text = "Reset to today", BackgroundColor = Color.FromArgb("#18796B"), TextColor = Colors.White, CornerRadius = 14, Padding = 16 };
        reset.Clicked += async (_, _) =>
        {
            if (!await DisplayAlertAsync("Reset the counter?", "Your last drink date will be set to today and the count will return to 0.", "Reset", "Cancel")) return;
            try { store.Reset(DateTime.Today); Refresh(); }
            catch (Exception ex) { await DisplayAlertAsync("Couldn't save", ex.Message, "OK"); }
        };
        var panel = new VerticalStackLayout { Spacing = 18, Padding = new Thickness(24, 40), MaximumWidthRequest = 520, VerticalOptions = LayoutOptions.Center,
            Children = { Text("DayCounter", 30), count, unit, since, Text("One day at a time.", 18), reset } };
#if ANDROID
        panel.Children.Add(Text("To see your count on your home screen, touch and hold an empty space, choose Widgets, then DayCounter. Tap the widget to open the app.", 13));
#endif
        Content = new ScrollView { Content = panel, VerticalOptions = LayoutOptions.Fill };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
        timer ??= Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(30);
        timer.Tick -= OnTick;
        timer.Tick += OnTick;
        timer.Start();
    }

    protected override void OnDisappearing() { timer?.Stop(); base.OnDisappearing(); }
    private void OnTick(object? sender, EventArgs args) => Refresh();
    private void Refresh()
    {
        try
        {
            var start = store.GetOrCreate(DateTime.Today);
            var days = DayCount.Since(start, DateTime.Today);
            count.Text = days.ToString();
            unit.Text = days == 1 ? "DAY SINCE LAST DRINK" : "DAYS SINCE LAST DRINK";
            since.Text = $"Since {start:d MMMM yyyy}";
#if ANDROID
            DayCounterWidget.UpdateAll(Android.App.Application.Context);
#endif
        }
        catch { count.Text = "—"; since.Text = "Couldn't read the saved date. Reset to today to start again."; }
    }

    private static Label Text(string value, double size) => new() { Text = value, FontSize = size, FontFamily = "OpenSansRegular", TextColor = Color.FromArgb("#183B36"), HorizontalTextAlignment = TextAlignment.Center };
}
