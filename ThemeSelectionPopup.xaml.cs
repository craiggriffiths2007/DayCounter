using CommunityToolkit.Maui.Views;
using DayCounter.Services;

namespace DayCounter;

public partial class ThemeSelectionPopup : Popup<string>
{
    public ThemeSelectionPopup(string currentTheme)
    {
        InitializeComponent();
        Highlight(WeatherButton, currentTheme == CounterSettings.Weather);
        Highlight(CoastalButton, currentTheme == CounterSettings.Coastal);
        Highlight(ClassicButton, currentTheme == CounterSettings.Classic);
    }

    private static void Highlight(Button button, bool selected)
    {
        button.BackgroundColor = Color.FromArgb(selected ? "#18796B" : "#E0E8E3");
        button.TextColor = selected ? Colors.White : Color.FromArgb("#183B36");
    }

    private async void OnWeatherClicked(object sender, EventArgs e) =>
        await CloseAsync(CounterSettings.Weather);

    private async void OnCoastalClicked(object? sender, EventArgs e) =>
        await CloseAsync(CounterSettings.Coastal);

    private async void OnClassicClicked(object sender, EventArgs e) =>
        await CloseAsync(CounterSettings.Classic);

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await CloseAsync(string.Empty);
}
