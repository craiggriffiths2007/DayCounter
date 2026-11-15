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
        ThemePicker.SelectedIndex = settings.Theme == CounterSettings.Classic ? 1 : 0;
        loading = false;
    }

    private void OnAllowDayChangeToggled(object sender, ToggledEventArgs e)
    {
        if (loading) return;
        settings.AllowDayChange = e.Value;
    }

    private void OnThemeChanged(object sender, EventArgs e)
    {
        if (loading || ThemePicker.SelectedIndex < 0) return;
        settings.Theme = ThemePicker.SelectedIndex == 1 ? CounterSettings.Classic : CounterSettings.Weather;
#if ANDROID
        DayCounterWidget.UpdateAll(Android.App.Application.Context);
#endif
    }

    private async void OnBackClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
