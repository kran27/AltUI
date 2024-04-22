using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace AltUI.Config
{
    public static class ThemeProvider
    {
        private static object RegistryCGV(string keyName, string valueName, object defaultValue)
        {
            try
            {
                return Registry.GetValue(keyName, valueName, defaultValue);
            }
            catch { return defaultValue; }
        }

        public static bool LightMode = (int)RegistryCGV("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", 0) == 1;
        public static bool TransparencyMode = (int)RegistryCGV("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "EnableTransparency", 0) == 1;
        public static int WindowsVersion = int.Parse((string)RegistryCGV("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuild", 0));

        public static Color BackgroundColour
        {
            get
            {
                if (LightMode)
                {
                    if (TransparencyMode & WindowsVersion >= 22000)
                    { return Color.FromArgb(231, 231, 232); }

                    return Color.FromArgb(255, 255, 255);
                }

                if (TransparencyMode & WindowsVersion >= 22000)
                { return Color.FromArgb(31, 31, 32); }

                return Color.FromArgb(16, 16, 17);
            }
        }

        private static readonly int AccentColour = (int)RegistryCGV("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\DWM", "AccentColor", 0);

        public static Color GetAccentColor(int brighten)
        {
            if (WindowsVersion >= 22000)
            {
                return ParseDWordColor(AccentColour, brighten);
            }

            if (brighten != 0)
                return Color.FromArgb(0, 120, 215);
            return Color.FromArgb(30, 71, 112);
        }

        private static Color ParseDWordColor(int color, int brighten)
        {
            int
                a = (color >> 24) & 0xFF, r = (color >> 0) & 0xFF, g = (color >> 8) & 0xFF, b = (color >> 16) & 0xFF;
            if (b + brighten > 255) { b = 255; }
            else { b += brighten; }
            if (g + brighten > 255) { g = 255; }
            else { g += brighten; }
            if (r + brighten > 255) { r = 255; }
            else { r += brighten; }
            return Color.FromArgb(a, r, g, b);
        }

        private static ITheme theme;

        public static ITheme Theme
        {
            get
            {
                if (LightMode)
                    theme = new LightTheme();
                else
                    theme = new DarkTheme();
                return theme;
            }
            set => theme = value;
        }

        private static int ToColorRef(this Color c) => Convert.ToInt32($"{c.B:X2}{c.G:X2}{c.R:X2}", 16);

        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attr, int[] attrValue, int attrSize);

        public static void SetupWindow(IntPtr Handle, int Corners, bool CustomBorder = true)
        {
            var MSOTOI = Marshal.SizeOf(typeof(int));
            // Use rounded corners on 22000+
            DwmSetWindowAttribute(Handle, 33, new[] { Corners }, MSOTOI);
            // Set Window border to match control border
            if (CustomBorder)
                DwmSetWindowAttribute(Handle, 34, new[] { Theme.Colors.GreySelection.ToColorRef() }, MSOTOI);
            // Set Window Caption to match background
            if (WindowsVersion < 22523 || !TransparencyMode)
                DwmSetWindowAttribute(Handle, 35, new[] { Theme.Colors.GreyBackground.ToColorRef() }, MSOTOI);
            // Set Titlebar Font to match Theme
            DwmSetWindowAttribute(Handle, 36, new[] { Theme.Colors.LightText.ToColorRef() }, MSOTOI);
            // Apply immersive dark mode if it's used by system
            DwmSetWindowAttribute(Handle, 20, new[] { LightMode ? 0 : 1 }, MSOTOI);
            if (TransparencyMode & WindowsVersion >= 22000)
            {
                // Mica for below 22523
                DwmSetWindowAttribute(Handle, 1029, new[] { 1 }, MSOTOI);
                // Mica for 22523 and up
                DwmSetWindowAttribute(Handle, 38, new[] { LightMode ? 4 : 2 }, MSOTOI);
            }
        }
    }

    public static class RoundRects
    {
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
        {
            var diameter = radius * 2;
            Size size = new(diameter, diameter);
            Rectangle arc = new(bounds.Location, size);
            GraphicsPath path = new();

            if (radius == 0 || flatBottom && flatTop)
            {
                path.AddRectangle(bounds);
                return path;
            }
            if (flatTop)
            {
                // top
                PointF tr = new(bounds.Right, bounds.Top);
                PointF tl = new(bounds.Left, bounds.Top);
                path.AddLine(tl, tr);

                arc.X = bounds.Right - diameter;
            }
            else
            {
                // top left arc
                path.AddArc(arc, 180, 90);

                // top right arc
                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);
            }
            if (flatBottom)
            {
                // bottom line
                PointF br = new(bounds.Right, bounds.Bottom + bottomOffset);
                PointF bl = new(bounds.Left, bounds.Bottom + bottomOffset);
                path.AddLine(br, bl);
            }
            else
            {
                // bottom right arc
                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                // bottom left arc
                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);
            }
            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using var path = RoundedRect(bounds, cornerRadius, flatBottom, bottomOffset, flatTop);
            graphics.DrawPath(pen, path);
        }

        public static void DrawGapRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius, int topGapStart, int topGapEnd)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            var diameter = cornerRadius * 2;
            Size size = new(diameter, diameter);
            Rectangle arc = new(bounds.Location, size);

            if (cornerRadius == 0)
            {
                graphics.DrawRectangle(pen, bounds);
                return;
            }

            // top left arc
            graphics.DrawArc(pen, arc, 180, 90);

            graphics.DrawLine(pen, bounds.X + cornerRadius, bounds.Y, topGapStart, bounds.Y);
            graphics.DrawLine(pen, topGapEnd, bounds.Y, bounds.Right - cornerRadius, bounds.Y);

            // top right arc
            arc.X = bounds.Right - diameter;
            graphics.DrawArc(pen, arc, 270, 90);

            graphics.DrawLine(pen, bounds.Right, bounds.Y + cornerRadius, bounds.Right, bounds.Bottom - cornerRadius);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            graphics.DrawArc(pen, arc, 0, 90);

            graphics.DrawLine(pen, bounds.Right - cornerRadius, bounds.Bottom, bounds.Left + cornerRadius, bounds.Bottom);

            // bottom left arc
            arc.X = bounds.Left;
            graphics.DrawArc(pen, arc, 90, 90);

            graphics.DrawLine(pen, bounds.Left, bounds.Bottom - cornerRadius, bounds.Left, bounds.Y + cornerRadius);
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (brush == null)
                throw new ArgumentNullException("brush");

            using var path = RoundedRect(bounds, cornerRadius, flatBottom, bottomOffset, flatTop);
            graphics.FillPath(brush, path);
        }

        public static void DrawRectangleCorners(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            var path = RoundedRect(bounds, cornerRadius);

            path.AddLine(bounds.Left, bounds.Top, bounds.Right, bounds.Top);
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();

            graphics.FillPath(brush, path);
        }

        public static void DrawCustomBorder(this Graphics graphics, Rectangle bounds, int cornerRadius)
        {
            SolidBrush brush = new(ThemeProvider.Theme.Colors.GreyBackground);
            Pen pen1 = new(ThemeProvider.Theme.Colors.GreyBackground);
            Pen pen2 = new(ThemeProvider.Theme.Colors.GreySelection);
            bounds.Width--;
            bounds.Height--;
            graphics.SmoothingMode = SmoothingMode.None;
            DrawRectangleCorners(graphics, brush, bounds, cornerRadius - 1);
            graphics.DrawRectangle(pen1, bounds);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawRoundedRectangle(graphics, pen2, bounds, cornerRadius);
        }
    }
}