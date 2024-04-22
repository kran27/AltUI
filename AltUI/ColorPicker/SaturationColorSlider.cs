// Cyotek Color Picker Controls Library
// http://cyotek.com/blog/tag/colorpicker

// Copyright © 2013-2021 Cyotek Ltd.

// This work is licensed under the MIT License.
// See LICENSE.TXT for the full text

// Found this code useful?
// https://www.cyotek.com/contribute

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AltUI.Config;

namespace AltUI.ColorPicker
{
    /// <summary>
    /// Represents a control for selecting the saturation of a color
    /// </summary>
    public class SaturationColorSlider : ColorSlider
    {
        #region Private Fields

        private static readonly object _eventColorChanged = new object();

        private Brush _cellBackgroundBrush;

        private Color _color;

        #endregion Private Fields

        #region Public Constructors

        public SaturationColorSlider()
        {
            BarStyle = ColorBarStyle.TwoColor;
            Color = Color.Black;
        }

        #endregion Public Constructors

        #region Public Events

        [Category("Property Changed")]
        public event EventHandler ColorChanged
        {
            add { Events.AddHandler(_eventColorChanged, value); }
            remove { Events.RemoveHandler(_eventColorChanged, value); }
        }

        #endregion Public Events

        #region Public Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ColorBarStyle BarStyle
        {
            get => base.BarStyle;
            set => base.BarStyle = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public virtual Color Color
        {
            get => _color;
            set
            {
                if (Color != value)
                {
                    _color = value;

                    OnColorChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color Color1
        {
            get => base.Color1;
            set => base.Color1 = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color Color2
        {
            get => base.Color2;
            set => base.Color2 = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color Color3
        {
            get => base.Color3;
            set => base.Color3 = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override float Maximum
        {
            get => base.Maximum;
            set => base.Maximum = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override float Minimum
        {
            get => base.Minimum;
            set => base.Minimum = value;
        }

        public override float Value
        {
            get => base.Value;
            set => base.Value = (int)value;
        }

        #endregion Public Properties

        #region Protected Methods

        protected virtual void CreateScale()
        {
            HslColor color;

            color = new HslColor(Color)
            {
                S = 0
            };

            Color1 = color.ToRgbColor();

            color.S = 1;
            Color2 = color.ToRgbColor();
        }

        protected virtual Brush CreateTransparencyBrush()
        {
            return new TextureBrush(ResourceManager.CellBackground, WrapMode.Tile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cellBackgroundBrush != null)
                {
                    _cellBackgroundBrush.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Raises the <see cref="ColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnColorChanged(EventArgs e)
        {
            EventHandler handler;

            CreateScale();
            Invalidate();

            handler = (EventHandler)Events[_eventColorChanged];

            handler?.Invoke(this, e);
        }

        protected override void PaintBar(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (Color.A != 255)
            {
                _cellBackgroundBrush ??= CreateTransparencyBrush();

                var rect = BarBounds with { X = BarBounds.X + 1, Y = BarBounds.Y + 1, Width = BarBounds.Width - 3, Height = BarBounds.Height - 3 };
                e.Graphics.FillRoundedRectangle(_cellBackgroundBrush, rect, 4);
            }

            base.PaintBar(e);
        }

        #endregion Protected Methods
    }
}