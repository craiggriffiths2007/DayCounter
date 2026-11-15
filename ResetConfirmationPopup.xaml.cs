using CommunityToolkit.Maui.Views;

namespace DayCounter;

/// <summary>
/// A styled confirmation that returns true only when Reset is pressed.
/// No reset logic runs inside the popup.
/// </summary>
public partial class ResetConfirmationPopup : Popup<bool>
{
    public ResetConfirmationPopup()
    {
        InitializeComponent();
    }

    private async void OnResetClicked(object sender, EventArgs e) =>
        await CloseAsync(true);

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await CloseAsync(false);
}
