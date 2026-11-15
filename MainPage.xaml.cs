using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

public partial class MainPage : ContentPage
{
    private readonly StartDateStore store = new(System.IO.Path.Combine(FileSystem.AppDataDirectory, "start-date.txt"));
    private IDispatcherTimer? timer;
#if DEBUG && !ANDROID
    private int? localPreviewDay;
#endif

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        WidgetHelpLabel.IsVisible = true;
#endif
#if DEBUG
        PreviewPrevious.IsVisible = true;
        PreviewNext.IsVisible = true;
#endif
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        // A typed CommunityToolkit popup returns true only for Reset.
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

        if (result.Result is not true)
            return;

        try
        {
            store.Reset(DateTime.Today);
#if DEBUG && ANDROID
            DayCounterWidget.SetPreviewDay(Android.App.Application.Context, -1);
#elif DEBUG
            localPreviewDay = null;
#endif
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

    private void OnPreviewPreviousClicked(object sender, EventArgs e) => AdjustPreview(-1);
    private void OnPreviewNextClicked(object sender, EventArgs e) => AdjustPreview(1);

    private void AdjustPreview(int delta)
    {
#if DEBUG
        try
        {
            var start = store.GetOrCreate(DateTime.Today);
            var realDays = DayCount.Since(start, DateTime.Today);
#if ANDROID
            var context = Android.App.Application.Context;
            var previous = DayCounterWidget.PreviewDay(context);
            DayCounterWidget.SetPreviewDay(context, Math.Clamp((previous >= 0 ? previous : realDays) + delta, 0, 10));
#else
            localPreviewDay = Math.Clamp((localPreviewDay ?? realDays) + delta, 0, 10);
#endif
            Refresh();
        }
        catch { Refresh(); }
#endif
    }

    private void Refresh()
    {
        try
        {
            var start = store.GetOrCreate(DateTime.Today);
            var realDays = DayCount.Since(start, DateTime.Today);
            var days = realDays;
#if DEBUG && ANDROID
            var preview = DayCounterWidget.PreviewDay(Android.App.Application.Context);
            if (preview >= 0) days = preview;
#elif DEBUG
            if (localPreviewDay.HasValue) days = localPreviewDay.Value;
#endif
            CountLabel.Text = days.ToString();
            UnitLabel.Text = days == 1 ? "DAY SINCE LAST DRINK" : "DAYS SINCE LAST DRINK";
            SceneImage.Source = $"day_scene_{Math.Clamp(days, 0, 10)}.png";
            MessageLabel.Text = MessageForDay(days);
            SinceLabel.Text = $"Since {start:d MMMM yyyy}";
#if DEBUG
            if (days != realDays)
                SinceLabel.Text += $"  (Preview day {days}; date unchanged)";
#endif
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
