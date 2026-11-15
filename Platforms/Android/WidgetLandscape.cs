#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Content;
using AColor = Android.Graphics.Color;

namespace DayCounter;

/// <summary>Draws a compact, offline weather-to-sunshine landscape for the Android widget.</summary>
internal static class WidgetLandscape
{
    private const int Size = 320;

    internal static int Stage(int days) => Math.Clamp(days, 0, 10);

    internal static string Message(int days) => Stage(days) switch
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

    internal static Bitmap Render(int days)
    {
        var stage = Stage(days);
        var progress = stage / 10f;
        var bitmap = Bitmap.CreateBitmap(Size, Size, Bitmap.Config.Argb8888!);
        using var canvas = new Canvas(bitmap);
        using var paint = new Paint(PaintFlags.AntiAlias);
        using var clip = new Path();
        clip.AddRoundRect(new RectF(0, 0, Size, Size), 40, 40, Path.Direction.Cw);
        canvas.ClipPath(clip);

        // A brighter sky and more vivid hills appear as the streak grows.
        var top = Mix(AColor.Rgb(29, 43, 60), AColor.Rgb(54, 160, 230), progress);
        var bottom = Mix(AColor.Rgb(82, 92, 102), AColor.Rgb(246, 212, 136), progress);
        using (var sky = new LinearGradient(0, 0, 0, 285, top, bottom, Shader.TileMode.Clamp!))
        {
            paint.SetShader(sky);
            canvas.DrawRect(0, 0, Size, Size, paint);
            paint.SetShader(null);
        }

        // The sun slowly emerges from behind the clouds.
        if (stage >= 3)
        {
            var sunVisibility = Math.Min(1f, (stage - 2) / 5f);
            paint.Color = AColor.Argb((int)(65 * sunVisibility), 255, 237, 174);
            canvas.DrawCircle(245, 104, 59, paint);
            paint.Color = AColor.Argb((int)(235 * sunVisibility), 255, 242, 171);
            canvas.DrawCircle(245, 104, 31, paint);
        }

        // Clouds recede across the same landscape day by day.
        var cloudOpacity = (int)(205 * (1 - progress));
        if (cloudOpacity > 0)
        {
            Cloud(canvas, paint, 80 - stage * 6, 74, 1.2f, cloudOpacity);
            Cloud(canvas, paint, 225 - stage * 3, 145, 0.85f, cloudOpacity * 3 / 4);
            if (stage < 4) Cloud(canvas, paint, 190, 46, 1.1f, cloudOpacity);
        }

        if (stage <= 2)
        {
            paint.Color = AColor.Argb((int)(125 * (3 - stage) / 3f), 202, 221, 230);
            for (var i = 0; i < 13; i++)
            {
                var x = (i * 79 + 23) % 320;
                var y = 96 + (i * 43) % 108;
                paint.StrokeWidth = 2.4f;
                canvas.DrawLine(x, y, x - 8, y + 19, paint);
            }
        }

        Hill(canvas, paint, Mix(AColor.Rgb(68, 77, 83), AColor.Rgb(83, 143, 111), progress),
            new[] { 0f, 222f, 75f, 192f, 145f, 220f, 244f, 182f, 320f, 219f });
        Hill(canvas, paint, Mix(AColor.Rgb(45, 62, 65), AColor.Rgb(48, 126, 81), progress),
            new[] { 0f, 269f, 79f, 241f, 155f, 256f, 242f, 225f, 320f, 252f });
        Hill(canvas, paint, Mix(AColor.Rgb(32, 48, 51), AColor.Rgb(31, 103, 66), progress),
            new[] { 0f, 294f, 96f, 280f, 201f, 299f, 320f, 267f });

        if (stage >= 8)
        {
            paint.Color = AColor.Rgb(255, 221, 129);
            for (var i = 0; i < 7; i++)
            {
                var x = 25 + i * 44;
                var y = 286 + i % 3 * 8;
                canvas.DrawCircle(x, y, 2.5f, paint);
            }
        }

        // A subtle darkening along the lower edge helps the text stand out.
        using (var shade = new LinearGradient(0, 115, 0, 320,
                   AColor.Argb(0, 8, 24, 33), AColor.Argb(170, 8, 24, 33),
                   Shader.TileMode.Clamp!))
        {
            paint.SetShader(shade);
            canvas.DrawRect(0, 115, 320, 320, paint);
            paint.SetShader(null);
        }
        return bitmap;
    }

    private static void Cloud(Canvas canvas, Paint paint, float x, float y, float scale, int alpha)
    {
        paint.Color = AColor.Argb(Math.Clamp(alpha, 0, 255), 227, 231, 231);
        canvas.DrawOval(new RectF(x - 46 * scale, y - 4 * scale, x + 48 * scale, y + 16 * scale), paint);
        canvas.DrawCircle(x - 20 * scale, y, 22 * scale, paint);
        canvas.DrawCircle(x + 9 * scale, y - 12 * scale, 28 * scale, paint);
        canvas.DrawCircle(x + 35 * scale, y + 2 * scale, 18 * scale, paint);
    }

    private static void Hill(Canvas canvas, Paint paint, AColor color, float[] p)
    {
        paint.Color = color;
        using var shape = new Path();
        shape.MoveTo(p[0], p[1]);
        shape.QuadTo(p[2], p[3], p[4], p[5]);
        shape.QuadTo(p[6], p[7], p[8], p[9]);
        shape.LineTo(320, 320);
        shape.LineTo(0, 320);
        shape.Close();
        canvas.DrawPath(shape, paint);
    }

    private static AColor Mix(AColor a, AColor b, float amount)
    {
        int Blend(int x, int y) => (int)Math.Round(x + (y - x) * amount);
        return AColor.Rgb(Blend(a.R, b.R), Blend(a.G, b.G), Blend(a.B, b.B));
    }
}
#endif
