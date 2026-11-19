using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

public partial class MainPage : ContentPage
{
    private readonly StartDateStore store = new(System.IO.Path.Combine(FileSystem.AppDataDirectory, "start-date.txt"));
    private readonly CounterSettings settings = new(FileSystem.AppDataDirectory);
    private IDispatcherTimer? timer;

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        WidgetHelpLabel.IsVisible = true;
#endif
    }

    private async void OnSettingsClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(SettingsPage));

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<bool>(
            new ResetConfirmationPopup(),
            new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(20),
                    StrokeThickness = 0
                }
            },
            CancellationToken.None);

        if (result.Result is not true) return;

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

    private void OnPreviewPreviousClicked(object sender, EventArgs e) => ChangeDays(-1);
    private void OnPreviewNextClicked(object sender, EventArgs e) => ChangeDays(1);

    private void ChangeDays(int delta)
    {
        // Never change the saved date unless explicitly enabled in Settings.
        if (!settings.AllowDayChange) return;
        try
        {
            var oldDays = DayCount.Since(store.GetOrCreate(DateTime.Today), DateTime.Today);
            var nextDays = Math.Clamp(oldDays + delta, 0, 36500);
            if (nextDays != oldDays)
                store.Reset(DateTime.Today.AddDays(-nextDays));
            Refresh();
        }
        catch (Exception ex)
        {
            _ = DisplayAlertAsync("Couldn't change day", ex.Message, "OK");
        }
    }

    private void Refresh()
    {
        try
        {
            var start = store.GetOrCreate(DateTime.Today);
            var days = DayCount.Since(start, DateTime.Today);

            PreviewPrevious.IsVisible = settings.AllowDayChange;
            PreviewNext.IsVisible = settings.AllowDayChange;
            CountLabel.Text = days.ToString();
            UnitLabel.Text = days == 1 ? "DAY SINCE LAST DRINK" : "DAYS SINCE LAST DRINK";
            SinceLabel.Text = $"Since {start:d MMMM yyyy}";

            var theme = settings.Theme;
            SceneImage.IsVisible = theme != CounterSettings.Classic;
            if (SceneImage.IsVisible)
            {
                SceneImage.Source = theme == CounterSettings.Coastal
                    ? $"coastal_path_{CounterSettings.CoastalStage(days)}.jpg"
                    : $"day_scene_{Math.Clamp(days, 0, 10)}.png";
            }
            MessageLabel.Text = theme == CounterSettings.Weather
                ? MessageForDay(days) : "One day at a time";
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

    private static string MessageForDay(int days) => Math.Clamp(days, 0, 10) switch
    {
        0 => "A fresh start",
        1 => "One day stronger",
        2 => "Finding your feet",
        3 => "Clouds are lifting",
        4 => "Feeling brighter",
        5 => "Halfway to sunshine",
        6 => "Building momentum",
        7 => "One whole week!",
        8 => "Growing stronger",
        9 => "Look how far you've come",
        _ => "Clear skies ahead"
    };
}
