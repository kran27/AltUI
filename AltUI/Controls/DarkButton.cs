using AltUI.Config;
using AltUI.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AltUI.Controls
{
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class DarkButton : Button
    {
        #region Field Region

        private DarkButtonStyle _style = DarkButtonStyle.Normal;
        private DarkControlState _buttonState = DarkControlState.Normal;

        private bool _isDefault;
        private bool _spacePressed;
        private bool _flatBottom;
        private bool _flatTop;
        private bool _customColour;

        private Color _borderColour;

        private int _padding = ThemeProvider.Theme.Sizes.Padding / 2;
        private int _imagePadding = 5; // ThemeProvider.Theme.Sizes.Padding / 2

        #endregion Field Region

        #region Designer Property Region

        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the style of the button.")]
        [DefaultValue(DarkButtonStyle.Normal)]
        public DarkButtonStyle ButtonStyle
        {
            get => _style;
            set
            {
                _style = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines the amount of padding between the image and text.")]
        [DefaultValue(5)]
        public int ImagePadding
        {
            get => _imagePadding;
            set
            {
                _imagePadding = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether or not the button has a flat top.")]
        [DefaultValue(5)]
        public bool FlatTop
        {
            get => _flatTop;
            set
            {
                _flatTop = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether or not the button has a flat bottom.")]
        [DefaultValue(5)]
        public bool FlatBottom
        {
            get => _flatBottom;
            set
            {
                _flatBottom = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether or not the button has a custom coloured border.")]
        [DefaultValue(5)]
        public bool CustomColour
        {
            get => _customColour;
            set
            {
                _customColour = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether or not the button has a coloured border.")]
        [DefaultValue(5)]
        public Color BorderColour
        {
            get => _borderColour;
            set
            {
                _borderColour = value;
                Invalidate();
            }
        }

        #endregion Designer Property Region

        #region Code Property Region

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoEllipsis => false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkControlState ButtonState => _buttonState;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment ImageAlign => base.ImageAlign;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FlatAppearance => false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FlatStyle FlatStyle => base.FlatStyle;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment TextAlign => base.TextAlign;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseCompatibleTextRendering => false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseVisualStyleBackColor => false;

        #endregion Code Property Region

        #region Constructor Region

        public DarkButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            base.UseVisualStyleBackColor = false;
            base.UseCompatibleTextRendering = false;

            SetButtonState(DarkControlState.Normal);
            Padding = new Padding(_padding);
        }

        #endregion Constructor Region

        #region Method Region

        private void SetButtonState(DarkControlState buttonState)
        {
            if (_buttonState != buttonState)
            {
                _buttonState = buttonState;
                Invalidate();
            }
        }

        #endregion Method Region

        #region Event Handler Region

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            var form = FindForm();
            if (form != null)
            {
                if (form.AcceptButton == this)
                    _isDefault = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_spacePressed)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (ClientRectangle.Contains(e.Location))
                    SetButtonState(DarkControlState.Pressed);
                else
                    SetButtonState(DarkControlState.Hover);
            }
            else
            {
                SetButtonState(DarkControlState.Hover);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!ClientRectangle.Contains(e.Location))
                return;

            SetButtonState(DarkControlState.Pressed);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_spacePressed)
                return;

            SetButtonState(DarkControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (_spacePressed)
                return;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            _spacePressed = false;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(DarkControlState.Normal);
            else
                SetButtonState(DarkControlState.Hover);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = true;
                SetButtonState(DarkControlState.Pressed);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = false;

                var location = Cursor.Position;

                if (!ClientRectangle.Contains(location))
                    SetButtonState(DarkControlState.Normal);
                else
                    SetButtonState(DarkControlState.Hover);
            }
        }

        public override void NotifyDefault(bool value)
        {
            base.NotifyDefault(value);

            if (!DesignMode)
                return;

            _isDefault = value;
            Invalidate();
        }

        #endregion Event Handler Region

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var textColor = ThemeProvider.Theme.Colors.LightText;
            var borderColor = ThemeProvider.Theme.Colors.GreySelection;
            var fillColor = ThemeProvider.Theme.Colors.LightBackground;
            var overlayColor = Color.Transparent;

            if (Enabled)
            {
                if (Focused && TabStop || _customColour)
                {
                    BringToFront();
                    borderColor = _customColour ? _borderColour : ThemeProvider.Theme.Colors.BlueHighlight;
                }

                if (ButtonStyle == DarkButtonStyle.Normal)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Hover:
                            fillColor = ThemeProvider.Theme.Colors.LighterBackground;
                            break;

                        case DarkControlState.Pressed:
                            fillColor = ThemeProvider.Theme.Colors.DarkBackground;
                            break;
                    }
                }
                else if (ButtonStyle == DarkButtonStyle.Flat)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Normal:
                            fillColor = ThemeProvider.Theme.Colors.GreyBackground;
                            break;

                        case DarkControlState.Hover:
                            fillColor = ThemeProvider.Theme.Colors.MediumBackground;
                            break;

                        case DarkControlState.Pressed:
                            fillColor = ThemeProvider.Theme.Colors.DarkBackground;
                            break;
                    }
                }
                else if (ButtonStyle == DarkButtonStyle.Image)
                {
                    switch (ButtonState)
                    {
                        case DarkControlState.Normal:
                            overlayColor = Color.Transparent;
                            break;

                        case DarkControlState.Hover:
                            overlayColor = Color.FromArgb(27, 242, 242, 255);
                            break;

                        case DarkControlState.Pressed:
                            overlayColor = Color.FromArgb(57, 255, 255, 246);
                            break;
                    }
                }
            }
            else
            {
                textColor = ThemeProvider.Theme.Colors.DisabledText;
                fillColor = ThemeProvider.Theme.Colors.DarkGreySelection;
            }

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
                g.FillRoundedRectangle(b, modRect, 4, _flatBottom, 0, _flatTop);
                g.SmoothingMode = SmoothingMode.None;
            }

            if (ButtonStyle == DarkButtonStyle.Image && BackgroundImage != null)
            {
                var imgRect = new Rectangle(rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2);
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.DrawImage(BackgroundImage, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height);
                g.DrawRectangleCorners(new SolidBrush(ThemeProvider.Theme.Colors.LightBackground), rect, 4);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var b = new SolidBrush(overlayColor);
                g.FillRoundedRectangle(b, modRect, 4);
            }

            if (ButtonStyle is DarkButtonStyle.Normal or DarkButtonStyle.Image)
            {
                using var p = new Pen(borderColor, 1);
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRoundedRectangle(p, modRect, 4, _flatBottom, 0, _flatTop);
                g.SmoothingMode = SmoothingMode.None;
            }

            var textOffsetX = 0;
            var textOffsetY = 0;

            if (Image != null)
            {
                var stringSize = g.MeasureString(Text, Font, rect.Size);

                var x = (ClientSize.Width / 2) - (Image.Size.Width / 2);
                var y = (ClientSize.Height / 2) - (Image.Size.Height / 2);

                switch (TextImageRelation)
                {
                    case TextImageRelation.ImageAboveText:
                        textOffsetY = (Image.Size.Height / 2) + (ImagePadding / 2);
                        y -= ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;

                    case TextImageRelation.TextAboveImage:
                        textOffsetY = ((Image.Size.Height / 2) + (ImagePadding / 2)) * -1;
                        y += ((int)(stringSize.Height / 2) + (ImagePadding / 2));
                        break;

                    case TextImageRelation.ImageBeforeText:
                        textOffsetX = Image.Size.Width + (ImagePadding * 2);
                        x = ImagePadding;
                        break;

                    case TextImageRelation.TextBeforeImage:
                        x += (int)stringSize.Width;
                        break;
                }
                g.DrawImageUnscaled(Image, x, y);
            }

            using (var b = new SolidBrush(textColor))
            {
                var modRect = new Rectangle(rect.Left + textOffsetX + Padding.Left,
                                            rect.Top + textOffsetY + Padding.Top, rect.Width - Padding.Horizontal,
                                            rect.Height - Padding.Vertical);

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(Text, Font, b, modRect, stringFormat);
            }
        }

        #endregion Paint Region
    }
}