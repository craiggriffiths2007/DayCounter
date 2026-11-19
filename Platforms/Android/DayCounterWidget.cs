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
            var store = new StartDateStore(System.IO.Path.Combine(context.FilesDir!.AbsolutePath, "start-date.txt"));
            days = DayCount.Since(store.GetOrCreate(DateTime.Today), DateTime.Today);
            value = days.ToString();
        }
        catch (Exception ex)
        {
            Log.Warn("DayCounterWidget", $"Unable to read counter date: {ex}");
            value = "—";
        }

        var settings = new CounterSettings(context.FilesDir!.AbsolutePath);
        var theme = settings.Theme;
        int[] scenes =
        {
            Resource.Drawable.widget_scene_0, Resource.Drawable.widget_scene_1,
            Resource.Drawable.widget_scene_2, Resource.Drawable.widget_scene_3,
            Resource.Drawable.widget_scene_4, Resource.Drawable.widget_scene_5,
            Resource.Drawable.widget_scene_6, Resource.Drawable.widget_scene_7,
            Resource.Drawable.widget_scene_8, Resource.Drawable.widget_scene_9,
            Resource.Drawable.widget_scene_10
        };
        int[] coastalScenes =
        {
            Resource.Drawable.coastal_path_0, Resource.Drawable.coastal_path_1,
            Resource.Drawable.coastal_path_2, Resource.Drawable.coastal_path_3,
            Resource.Drawable.coastal_path_4, Resource.Drawable.coastal_path_5,
            Resource.Drawable.coastal_path_6, Resource.Drawable.coastal_path_7,
            Resource.Drawable.coastal_path_8, Resource.Drawable.coastal_path_9,
            Resource.Drawable.coastal_path_10, Resource.Drawable.coastal_path_11
        };
        var imageResource = theme switch
        {
            CounterSettings.Classic => Resource.Drawable.widget_background,
            CounterSettings.Coastal => coastalScenes[CounterSettings.CoastalStage(days)],
            _ => scenes[WidgetLandscape.Stage(days)]
        };

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
                var views = new RemoteViews(context.PackageName,
                    compact ? Resource.Layout.daycounter_widget_compact : Resource.Layout.daycounter_widget);
                views.SetTextViewText(Resource.Id.day_count, value);
                if (!compact)
                {
                    views.SetTextViewText(Resource.Id.day_label,
                        value == "1" ? "day since last drink" : "days since last drink");
                    views.SetTextViewText(Resource.Id.day_message,
                        value == "—" ? "Open app to check date" :
                        theme == CounterSettings.Weather ? WidgetLandscape.Message(days) : "One day at a time");
                }
                views.SetImageViewResource(Resource.Id.widget_scene, imageResource);
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
