using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AltUI.ColorPicker;
using AltUI.Config;
using AltUI.Forms;
#pragma warning disable CA1416

namespace AltUI.Controls
{
    public class DarkComboBox : ComboBox
    {
        #region Field Region

        private bool _autoExpanding;

        #endregion

        #region Property Region

        [Category("Appearance")]
        [Description("Determines whether the drop down list should automatically expand to fit items.")]
        [DefaultValue(false)]
        public bool AutoExpanding
        {
            get => _autoExpanding;
            set
            {
                _autoExpanding = value;
                Invalidate();
            }
        }
        #endregion

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color ForeColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ComboBoxStyle DropDownStyle { get; set; }

        private Bitmap _buffer;

        private bool clicked;

        private bool hover;

        public DarkComboBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            DrawMode = DrawMode.OwnerDrawVariable;

            base.FlatStyle = FlatStyle.Flat;
            base.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _buffer = null;

            base.Dispose(disposing);
        }

        protected override void OnTabStopChanged(EventArgs e)
        {
            base.OnTabStopChanged(e);
            Invalidate();
        }

        protected override void OnTabIndexChanged(EventArgs e)
        {
            base.OnTabIndexChanged(e);
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnTextUpdate(EventArgs e)
        {
            base.OnTextUpdate(e);
            Invalidate();
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            Invalidate();
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            PaintCombobox();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _buffer = null;
            Invalidate();
        }
        
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            _buffer = null;
            Invalidate();
        }
        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            if (_autoExpanding)
            {
                var width = DropDownWidth;
                var g = CreateGraphics();
                var font = Font;
                int newWidth;
                foreach (string s in Items)
                {
                    newWidth = (int)g.MeasureString(s, font).Width + 25;
                    if (newWidth > width)
                        width = newWidth;
                }
                DropDownWidth = width;
            }
            clicked = true;
            Invalidate();
        }
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            clicked = false;
            Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hover = true;
            Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = false;
            Invalidate();
        }
        private void PaintCombobox()
        {
            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
                _buffer = new Bitmap(1, 1);
            _buffer ??= new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using var g = Graphics.FromImage(_buffer);
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            var textColor = Enabled
                ? ThemeProvider.Theme.Colors.LightText
                : ThemeProvider.Theme.Colors.DisabledText;

            var borderColor = ThemeProvider.Theme.Colors.GreySelection;
            var fillColor = hover ? ThemeProvider.Theme.Colors.LighterBackground : ThemeProvider.Theme.Colors.LightBackground;
            var arrowColour = !(Focused && TabStop) ? ThemeProvider.Theme.Colors.GreyHighlight : ThemeProvider.Theme.Colors.BlueHighlight;

            if (Focused && TabStop)
                borderColor = ThemeProvider.Theme.Colors.BlueHighlight;

            if (Parent.GetType() == typeof(TabPage) || Parent.GetType() == typeof(DarkGroupBox) && ((DarkGroupBox)Parent).OpaqueBackground)
            {
                using var b = new SolidBrush(ThemeProvider.Theme.Colors.LightBackground);
                g.FillRectangle(b, rect);
            }
            else
            {
                using var b = new SolidBrush(ThemeProvider.Theme.Colors.GreyBackground);
                g.FillRectangle(b, rect);
            }

            using (var b = new SolidBrush(fillColor))
            {
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRoundedRectangle(b, modRect, 4, clicked, 1);
                g.SmoothingMode = SmoothingMode.None;
            }

            using (var p = new Pen(borderColor, 1))
            {
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRoundedRectangle(p, modRect, 4, clicked, 1);
                g.SmoothingMode = SmoothingMode.None;
            }
            using (var p = new Pen(arrowColour, 1))
            {
                var x = rect.Right - 8 - (ThemeProvider.Theme.Sizes.Padding / 2);
                var y = rect.Height / 2 - 2;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(p, x, y, x + 3, y +3);
                g.DrawLine(p, x + 3, y + 3, x + 6, y);
                g.SmoothingMode = SmoothingMode.None;
            }
            var text = SelectedItem != null ? SelectedItem.ToString() : Text;

            using (var b = new SolidBrush(textColor))
            {
                const int padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - 8 - (ThemeProvider.Theme.Sizes.Padding / 2) - (padding * 2),
                    rect.Height - (padding * 2));

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(text, Font, b, modRect, stringFormat);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_buffer == null)
                PaintCombobox();

            var g = e.Graphics;
            g.DrawImageUnscaled(_buffer, Point.Empty);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var rect = e.Bounds;

            var textColor = ThemeProvider.Theme.Colors.LightText;
            var fillColor = ThemeProvider.Theme.Colors.GreyBackground;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
                (e.State & DrawItemState.Focus) == DrawItemState.Focus ||
                (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect && !TabStop)
            {
                fillColor = ThemeProvider.Theme.Colors.BlueSelection;
                if (ThemeProvider.LightMode)
                    textColor = ThemeProvider.Theme.Colors.GreyBackground;
            }


            using (var b = new SolidBrush(fillColor))
            {
                g.FillRectangle(b, rect);
            }

            if (e.Index < 0 || e.Index >= Items.Count) return;
            {
                var text = Items[e.Index].ToString();

                using var b = new SolidBrush(textColor);
                var padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - (padding * 2),
                    rect.Height - (padding * 2));

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(text, Font, b, modRect, stringFormat);
            }
        }
    }
}
