using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using DayCounter.Models;
using DayCounter.Services;

namespace DayCounter;

[BroadcastReceiver(Label = "DayCounter", Exported = true)]
[IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE", "android.intent.action.DATE_CHANGED", "android.intent.action.TIME_SET", "android.intent.action.TIMEZONE_CHANGED", "android.intent.action.BOOT_COMPLETED", "android.intent.action.MY_PACKAGE_REPLACED" })]
[MetaData("android.appwidget.provider", Resource = "@xml/daycounter_widget_info")]
public sealed class DayCounterWidget : AppWidgetProvider
{
    // A 1x1 launcher cell is usually around 70dp. Keep very short or narrow
    // widget sizes number-only, including 2x1 and 1x2 configurations.
    private const int CompactThresholdDp = 110;

    public override void OnUpdate(Context? context, AppWidgetManager? manager, int[]? ids)
    {
        if (context is not null) UpdateAll(context);
    }

    public override void OnAppWidgetOptionsChanged(Context? context, AppWidgetManager? manager,
        int appWidgetId, Bundle? newOptions)
    {
        base.OnAppWidgetOptionsChanged(context, manager, appWidgetId, newOptions);
        if (context is not null) UpdateAll(context);
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);
        if (context is not null &&
            intent?.Action != AppWidgetManager.ActionAppwidgetUpdate &&
            intent?.Action != AppWidgetManager.ActionAppwidgetOptionsChanged)
            UpdateAll(context);
    }

    public static void UpdateAll(Context context)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null) return;

        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(DayCounterWidget)));
        var ids = manager.GetAppWidgetIds(component);
        if (ids is null || ids.Length == 0) return;

        string value;
        var days = 0;
        try
        {
            var store = new StartDateStore(Path.Combine(context.FilesDir!.AbsolutePath, "start-date.txt"));
            days = DayCount.Since(store.GetOrCreate(DateTime.Today), DateTime.Today);
            value = days.ToString();
        }
        catch (Exception ex)
        {
            Log.Warn("DayCounterWidget", $"Unable to read counter date: {ex}");
            value = "—";
        }

        // Android drawable resources are applied by the launcher itself.
        // This avoids transferring a bitmap through RemoteViews, which could
        // silently leave the original green fallback visible.
        var stage = WidgetLandscape.Stage(days);
        int[] sceneResources =
        {
            Resource.Drawable.widget_scene_0,
            Resource.Drawable.widget_scene_1,
            Resource.Drawable.widget_scene_2,
            Resource.Drawable.widget_scene_3,
            Resource.Drawable.widget_scene_4,
            Resource.Drawable.widget_scene_5,
            Resource.Drawable.widget_scene_6,
            Resource.Drawable.widget_scene_7,
            Resource.Drawable.widget_scene_8,
            Resource.Drawable.widget_scene_9,
            Resource.Drawable.widget_scene_10
        };
        var sceneResource = sceneResources[stage];

        {
            var launch = new Intent(context, typeof(MainActivity));
            launch.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
            var pending = PendingIntent.GetActivity(context, 0, launch,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            foreach (var id in ids)
            {
                try
                {
                    var options = manager.GetAppWidgetOptions(id);
                    var width = options?.GetInt(AppWidgetManager.OptionAppwidgetMinWidth, 140) ?? 140;
                    var height = options?.GetInt(AppWidgetManager.OptionAppwidgetMinHeight, 140) ?? 140;
                    var compact = width < CompactThresholdDp || height < CompactThresholdDp;
                    var layout = compact
                        ? Resource.Layout.daycounter_widget_compact
                        : Resource.Layout.daycounter_widget;
                    var views = new RemoteViews(context.PackageName, layout);

                    views.SetTextViewText(Resource.Id.day_count, value);
                    if (!compact)
                    {
                        views.SetTextViewText(Resource.Id.day_label,
                            value == "1" ? "day since last drink" : "days since last drink");
                        views.SetTextViewText(Resource.Id.day_message,
                            value == "—" ? "Open app to check date" : WidgetLandscape.Message(days));
                    }
                    views.SetImageViewResource(Resource.Id.widget_scene, sceneResource);
                    views.SetOnClickPendingIntent(Resource.Id.widget_root, pending);
                    manager.UpdateAppWidget(id, views);
                }
                catch (Exception ex)
                {
                    Log.Warn("DayCounterWidget", $"Unable to update widget {id}: {ex}");
                }
            }
        }
     }
}
