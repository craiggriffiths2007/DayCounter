using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

public partial class MainPage : ContentPage
{
    private readonly StartDateStore store = new(Path.Combine(FileSystem.AppDataDirectory, "start-date.txt"));
    private IDispatcherTimer? timer;

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        WidgetHelpLabel.IsVisible = true;
#endif
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        if (!await DisplayAlertAsync("Reset the counter?",
            "Your last drink date will be set to today and the count will return to 0.",
            "Reset", "Cancel"))
            return;

        try
        {
            store.Reset(DateTime.Today);
            Refresh();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Couldn't save", ex.Message, "OK");
        }
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

    protected override void OnDisappearing()
    {
        timer?.Stop();
        base.OnDisappearing();
    }

    private void OnTick(object? sender, EventArgs args) => Refresh();

    private void Refresh()
    {
        try
        {
            var start = store.GetOrCreate(DateTime.Today);
            var days = DayCount.Since(start, DateTime.Today);
            CountLabel.Text = days.ToString();
            UnitLabel.Text = days == 1 ? "DAY SINCE LAST DRINK" : "DAYS SINCE LAST DRINK";
            SinceLabel.Text = $"Since {start:d MMMM yyyy}";
#if ANDROID
            DayCounterWidget.UpdateAll(Android.App.Application.Context);
#endif
        }
        catch
        {
            CountLabel.Text = "—";
            SinceLabel.Text = "Couldn't read the saved date. Reset to today to start again.";
        }
    }
}
