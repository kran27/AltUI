using AltUI.Config;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AltUI.Controls
{
    public class DarkToolTip : ToolTip
    {
        public DarkToolTip()
        {
            this.OwnerDraw = true;
            this.Popup += new PopupEventHandler(this.OnPopup);
            this.Draw += new DrawToolTipEventHandler(this.OnDraw);
        }

        private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
        {
            var tooltipText = this.GetToolTip(e.AssociatedControl);
            var font = new Font("Segoe UI", 9F);
            var strSize = TextRenderer.MeasureText(tooltipText, font);
            var newLines = tooltipText.Count(c => c == '\n');
            if (newLines == 0) newLines = 1;
            e.ToolTipSize = e.ToolTipSize with { Width = strSize.Width + 6, Height = strSize.Height + 5 + 3 * newLines };
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e) // use this event to customise the tool tip
        {
            var g = e.Graphics;
            var font = new Font("Segoe UI", 9F);
            var strSize = g.MeasureString(e.ToolTipText, font);
            var bgRect = e.Bounds with { Width = e.Bounds.Width - 1, Height = e.Bounds.Height - 1 };

            g.FillRoundedRectangle(new SolidBrush(ThemeProvider.Theme.Colors.LightBackground), bgRect, 4);
            g.DrawRoundedRectangle(new Pen(ThemeProvider.Theme.Colors.LightBackground, 1), bgRect, 4);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawRoundedRectangle(new Pen(ThemeProvider.Theme.Colors.GreySelection, 1), bgRect, 4);

            g.DrawString(e.ToolTipText, font, new SolidBrush(ThemeProvider.Theme.Colors.LightText),
                new PointF(e.Bounds.X + 4, e.Bounds.Y + 3));
        }
    }
}