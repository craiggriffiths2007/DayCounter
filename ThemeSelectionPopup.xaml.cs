using CommunityToolkit.Maui.Views;
using DayCounter.Services;

namespace DayCounter;

public partial class ThemeSelectionPopup : Popup<string>
{
    public ThemeSelectionPopup(string currentTheme)
    {
        InitializeComponent();
        var weather = currentTheme != CounterSettings.Classic;
        WeatherButton.BackgroundColor = Color.FromArgb(weather ? "#18796B" : "#E0E8E3");
        WeatherButton.TextColor = weather ? Colors.White : Color.FromArgb("#183B36");
        ClassicButton.BackgroundColor = Color.FromArgb(weather ? "#E0E8E3" : "#18796B");
        ClassicButton.TextColor = weather ? Color.FromArgb("#183B36") : Colors.White;
    }

    private async void OnWeatherClicked(object sender, EventArgs e) =>
        await CloseAsync(CounterSettings.Weather);

    private async void OnClassicClicked(object sender, EventArgs e) =>
        await CloseAsync(CounterSettings.Classic);

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await CloseAsync(string.Empty);
}
