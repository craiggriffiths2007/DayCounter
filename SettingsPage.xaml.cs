using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using DayCounter.Services;

namespace DayCounter;

public partial class SettingsPage : ContentPage
{
    private readonly CounterSettings settings = new(FileSystem.AppDataDirectory);
    private bool loading;

    public SettingsPage()
    {
        InitializeComponent();
        loading = true;
        AllowDayChangeSwitch.IsToggled = settings.AllowDayChange;
        UpdateThemeLabel();
        loading = false;
    }

    private void UpdateThemeLabel() =>
        ThemeButton.Text = settings.Theme switch
        {
            CounterSettings.Classic => "Classic Green",
            CounterSettings.Coastal => "Coast",
            _ => "Storm to Sunshine"
        };

    private void OnAllowDayChangeToggled(object sender, ToggledEventArgs e)
    {
        if (loading) return;
        settings.AllowDayChange = e.Value;
    }

    private async void OnChooseThemeClicked(object sender, EventArgs e)
    {
        var result = await this.ShowPopupAsync<string>(
            new ThemeSelectionPopup(settings.Theme),
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

        if (result.Result is not (CounterSettings.Weather or CounterSettings.Classic or CounterSettings.Coastal))
            return;

        settings.Theme = result.Result;
        UpdateThemeLabel();
#if ANDROID
        DayCounterWidget.UpdateAll(Android.App.Application.Context);
#endif
    }

    private async void OnBackClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
