using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

[BroadcastReceiver(Label = "DayCounter", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE", "android.intent.action.DATE_CHANGED", "android.intent.action.TIME_SET", "android.intent.action.TIMEZONE_CHANGED", "android.intent.action.BOOT_COMPLETED", "android.intent.action.MY_PACKAGE_REPLACED" })]
[MetaData("android.appwidget.provider", Resource = "@xml/daycounter_widget_info")]
public sealed class DayCounterWidget : AppWidgetProvider
{
    public override void OnUpdate(Context? context, AppWidgetManager? manager, int[]? ids)
    {
        if (context is not null) UpdateAll(context);
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);
        if (context is not null && intent?.Action != AppWidgetManager.ActionAppwidgetUpdate) UpdateAll(context);
    }

    public static void UpdateAll(Context context)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;
        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(DayCounterWidget)));
        var ids = manager.GetAppWidgetIds(component);
        if (ids is null || ids.Length == 0) return;
        string value;
        try
        {
            var store = new StartDateStore(Path.Combine(context.FilesDir!.AbsolutePath, "start-date.txt"));
            value = DayCount.Since(store.GetOrCreate(DateTime.Today), DateTime.Today).ToString();
        }
        catch { value = "—"; }
        var views = new RemoteViews(context.PackageName, Resource.Layout.daycounter_widget);
        views.SetTextViewText(Resource.Id.day_count, value);
        views.SetTextViewText(Resource.Id.day_label, value == "1" ? "day since last drink" : "days since last drink");
        var launch = new Intent(context, typeof(MainActivity));
        launch.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
        var pending = PendingIntent.GetActivity(context, 0, launch, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        views.SetOnClickPendingIntent(Resource.Id.widget_root, pending);
        manager.UpdateAppWidget(ids, views);
    }
}
