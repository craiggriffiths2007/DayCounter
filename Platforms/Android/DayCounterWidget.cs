using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
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
#if DEBUG
    private const string PreviewPreference = "daycounter_debug_preview";
    private const string PreviewKey = "preview_day";
    private const string PreviousAction = "com.craiggriffiths.daycounter.DEBUG_PREVIOUS";
    private const string NextAction = "com.craiggriffiths.daycounter.DEBUG_NEXT";

    internal static int PreviewDay(Context context) =>
        context.GetSharedPreferences(PreviewPreference, FileCreationMode.Private)?.GetInt(PreviewKey, -1) ?? -1;

    internal static void SetPreviewDay(Context context, int day)
    {
        var editor = context.GetSharedPreferences(PreviewPreference, FileCreationMode.Private)?.Edit();
        editor?.PutInt(PreviewKey, day < 0 ? -1 : Math.Clamp(day, 0, 10));
        editor?.Apply();
        UpdateAll(context);
    }
#endif

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
#if DEBUG
        if (context is not null && (intent?.Action == PreviousAction || intent?.Action == NextAction))
        {
            // Preview changes only the debug preference, never start-date.txt.
            var existing = PreviewDay(context);
            if (existing < 0)
            {
                var store = new StartDateStore(Path.Combine(context.FilesDir!.AbsolutePath, "start-date.txt"));
                existing = DayCount.Since(store.GetOrCreate(DateTime.Today), DateTime.Today);
            }
            SetPreviewDay(context, existing + (intent.Action == NextAction ? 1 : -1));
            return;
        }
#endif
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

#if DEBUG
        var preview = PreviewDay(context);
        if (preview >= 0)
        {
            days = preview;
            value = preview.ToString();
        }
#endif

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
#if DEBUG
                    views.SetViewVisibility(Resource.Id.debug_previous, ViewStates.Visible);
                    views.SetViewVisibility(Resource.Id.debug_next, ViewStates.Visible);
                    var prevIntent = new Intent(context, typeof(DayCounterWidget)).SetAction(PreviousAction);
                    var nextIntent = new Intent(context, typeof(DayCounterWidget)).SetAction(NextAction);
                    views.SetOnClickPendingIntent(Resource.Id.debug_previous,
                        PendingIntent.GetBroadcast(context, 401, prevIntent,
                            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable));
                    views.SetOnClickPendingIntent(Resource.Id.debug_next,
                        PendingIntent.GetBroadcast(context, 402, nextIntent,
                            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable));
#endif
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
