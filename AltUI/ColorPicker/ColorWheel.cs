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

namespace  AltUI.ColorPicker
{
  [DefaultProperty("Color")]
  [DefaultEvent("ColorChanged")]
  [ToolboxBitmap(typeof(ColorWheel), "ColorWheelToolboxBitmap.bmp")]
  [ToolboxItem(true)]
  public class ColorWheel : Control, IColorEditor
  {
#if !NET46_OR_GREATER
    private static readonly HslColor[] _empty = Array.Empty<HslColor>();
#endif

    #region Private Fields

    private static readonly object _eventAlphaChanged = new object();

    private static readonly object _eventColorChanged = new object();

    private static readonly object _eventColorStepChanged = new object();

    private static readonly object _eventDisplayLightnessChanged = new object();

    private static readonly object _eventHslColorChanged = new object();

    private static readonly object _eventLargeChangeChanged = new object();

    private static readonly object _eventLightnessChanged = new object();

    private static readonly object _eventLineColorChanged = new object();

    private static readonly object _eventSecondarySelectionSizeChanged = new object();

    private static readonly object _eventSelectionSizeChanged = new object();

    private static readonly object _eventShowAngleArrowChanged = new object();

    private static readonly object _eventShowCenterLinesChanged = new object();

    private static readonly object _eventShowSaturationRingChanged = new object();

    private static readonly object _eventSmallChangeChanged = new object();

    private double _alpha;

    private Point[] _arrowHead;

    private Brush _brush;

    private PointF _centerPoint;

    private Color _color;

    private Color[] _colors;

    private int _colorStep;

    private bool _displayLightness;

    private HslColor _hslColor;

    private bool _isDragging;

    private int _largeChange;

    private double _lightness;

    private Color _lineColor;

    private Pen _linePen;

    private bool _lockUpdates;

    private PointF[] _points;

    private float _radius;

    private HslColor[] _secondaryColors;

    private int _secondarySelectionSize;

    private Image _selectionGlyph;

    private int _selectionSize;

    private bool _showAngleArrow;

    private bool _showCenterLines;

    private bool _showSaturationRing;

    private int _smallChange;

    private int _updateCount;

    #endregion Private Fields

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorWheel"/> class.
    /// </summary>
    public ColorWheel()
    {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
      _color = Color.Black;
      _hslColor = new HslColor(_color)
      {
        L = 0.5
      };
      _colorStep = 4;
      _selectionSize = 10;
      _smallChange = 1;
      _largeChange = 5;
      _lightness = 0.5;
      _alpha = 1;
      _lineColor = Color.DimGray;
      _secondaryColors = GetEmptyColorArray();
      _secondarySelectionSize = 8;

      CreateLinePen();
    }

    #endregion Public Constructors

    #region Public Events

    /// <summary>
    /// Occurs when the Alpha property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler AlphaChanged
    {
      add => Events.AddHandler(_eventAlphaChanged, value);
      remove => Events.RemoveHandler(_eventAlphaChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler ColorChanged
    {
      add => Events.AddHandler(_eventColorChanged, value);
      remove => Events.RemoveHandler(_eventColorChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler ColorStepChanged
    {
      add => Events.AddHandler(_eventColorStepChanged, value);
      remove => Events.RemoveHandler(_eventColorStepChanged, value);
    }

    /// <summary>
    /// Occurs when the DisplayLightness property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler DisplayLightnessChanged
    {
      add => Events.AddHandler(_eventDisplayLightnessChanged, value);
      remove => Events.RemoveHandler(_eventDisplayLightnessChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler HslColorChanged
    {
      add => Events.AddHandler(_eventHslColorChanged, value);
      remove => Events.RemoveHandler(_eventHslColorChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler LargeChangeChanged
    {
      add => Events.AddHandler(_eventLargeChangeChanged, value);
      remove => Events.RemoveHandler(_eventLargeChangeChanged, value);
    }

    /// <summary>
    /// Occurs when the Lightness property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler LightnessChanged
    {
      add => Events.AddHandler(_eventLightnessChanged, value);
      remove => Events.RemoveHandler(_eventLightnessChanged, value);
    }

    /// <summary>
    /// Occurs when the LineColor property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler LineColorChanged
    {
      add => Events.AddHandler(_eventLineColorChanged, value);
      remove => Events.RemoveHandler(_eventLineColorChanged, value);
    }

    /// <summary>
    /// Occurs when the SecondarySelectionSize property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler SecondarySelectionSizeChanged
    {
      add => Events.AddHandler(_eventSecondarySelectionSizeChanged, value);
      remove => Events.RemoveHandler(_eventSecondarySelectionSizeChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler SelectionSizeChanged
    {
      add => Events.AddHandler(_eventSelectionSizeChanged, value);
      remove => Events.RemoveHandler(_eventSelectionSizeChanged, value);
    }

    /// <summary>
    /// Occurs when the ShowAngleArrow property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler ShowAngleArrowChanged
    {
      add => Events.AddHandler(_eventShowAngleArrowChanged, value);
      remove => Events.RemoveHandler(_eventShowAngleArrowChanged, value);
    }

    /// <summary>
    /// Occurs when the ShowCenterLines property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler ShowCenterLinesChanged
    {
      add => Events.AddHandler(_eventShowCenterLinesChanged, value);
      remove => Events.RemoveHandler(_eventShowCenterLinesChanged, value);
    }

    /// <summary>
    /// Occurs when the ShowSaturationRing property value changes
    /// </summary>
    [Category("Property Changed")]
    public event EventHandler ShowSaturationRingChanged
    {
      add => Events.AddHandler(_eventShowSaturationRingChanged, value);
      remove => Events.RemoveHandler(_eventShowSaturationRingChanged, value);
    }

    [Category("Property Changed")]
    public event EventHandler SmallChangeChanged
    {
      add => Events.AddHandler(_eventSmallChangeChanged, value);
      remove => Events.RemoveHandler(_eventSmallChangeChanged, value);
    }

    #endregion Public Events

    #region Public Properties

    [Category("Behavior")]
    [DefaultValue(1)]
    public double Alpha
    {
      get => _alpha;
      set
      {
        if (value < 0 || value > 1)
        {
          throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 1.");
        }

        if (Math.Abs(_alpha - value) > double.Epsilon)
        {
          _alpha = value;

          OnAlphaChanged(EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets the component color.
    /// </summary>
    /// <value>The component color.</value>
    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Black")]
    public virtual Color Color
    {
      get => _color;
      set
      {
        if (_color != value)
        {
          _color = value;

          OnColorChanged(EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets the increment for rendering the color wheel.
    /// </summary>
    /// <value>The color step.</value>
    /// <exception cref="System.ArgumentOutOfRangeException">Value must be between 1 and 359</exception>
    [Category("Appearance")]
    [DefaultValue(4)]
    public virtual int ColorStep
    {
      get => _colorStep;
      set
      {
        if (value < 1 || value > 359)
        {
          throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 1 and 359");
        }

        if (_colorStep != value)
        {
          _colorStep = value;

          OnColorStepChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool DisplayLightness
    {
      get => _displayLightness;
      set
      {
        if (_displayLightness != value)
        {
          _displayLightness = value;

          OnDisplayLightnessChanged(EventArgs.Empty);
        }
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Font Font
    {
      get => base.Font;
      set => base.Font = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color ForeColor
    {
      get => base.ForeColor;
      set => base.ForeColor = value;
    }

    /// <summary>
    /// Gets or sets the component color.
    /// </summary>
    /// <value>The component color.</value>
    [Category("Appearance")]
    [DefaultValue(typeof(HslColor), "0, 0, 0")]
    [Browsable(false) /* disable editing until I write a proper type convertor */]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual HslColor HslColor
    {
      get => _hslColor;
      set
      {
        if (_hslColor != value)
        {
          _hslColor = value;

          OnHslColorChanged(EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value to be added to or subtracted from the <see cref="Color"/> property when the wheel selection is moved a large distance.
    /// </summary>
    /// <value>A numeric value. The default value is 5.</value>
    [Category("Behavior")]
    [DefaultValue(5)]
    public virtual int LargeChange
    {
      get => _largeChange;
      set
      {
        if (_largeChange != value)
        {
          _largeChange = value;

          OnLargeChangeChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(0.5)]
    public double Lightness
    {
      get => _lightness;
      set
      {
        if (value < 0 || value > 1)
        {
          throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 1.");
        }

        if (Math.Abs(_lightness - value) > double.Epsilon)
        {
          _lightness = value;

          _hslColor = new HslColor(_hslColor.H, _hslColor.S, value);

          OnLightnessChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "DimGray")]
    public Color LineColor
    {
      get => _lineColor;
      set
      {
        if (_lineColor != value)
        {
          _lineColor = value;

          CreateLinePen();

          OnLineColorChanged(EventArgs.Empty);
        }
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HslColor[] SecondaryColors
    {
      get => _secondaryColors;
      set
      {
        _secondaryColors = value ?? GetEmptyColorArray();

        Invalidate();
      }
    }

    [Category("Appearance")]
    [DefaultValue(8)]
    public int SecondarySelectionSize
    {
      get => _secondarySelectionSize;
      set
      {
        if (_secondarySelectionSize != value)
        {
          _secondarySelectionSize = value;

          OnSecondarySelectionSizeChanged(EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets the size of the selection handle.
    /// </summary>
    /// <value>The size of the selection handle.</value>
    [Category("Appearance")]
    [DefaultValue(10)]
    public virtual int SelectionSize
    {
      get => _selectionSize;
      set
      {
        if (_selectionSize != value)
        {
          _selectionSize = value;

          OnSelectionSizeChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ShowAngleArrow
    {
      get => _showAngleArrow;
      set
      {
        if (_showAngleArrow != value)
        {
          _showAngleArrow = value;

          OnShowAngleArrowChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ShowCenterLines
    {
      get => _showCenterLines;
      set
      {
        if (_showCenterLines != value)
        {
          _showCenterLines = value;

          CreateLinePen();

          OnShowCenterLinesChanged(EventArgs.Empty);
        }
      }
    }

    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ShowSaturationRing
    {
      get => _showSaturationRing;
      set
      {
        if (_showSaturationRing != value)
        {
          _showSaturationRing = value;

          OnShowSaturationRingChanged(EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value to be added to or subtracted from the <see cref="Color"/> property when the wheel selection is moved a small distance.
    /// </summary>
    /// <value>A numeric value. The default value is 1.</value>
    [Category("Behavior")]
    [DefaultValue(1)]
    public virtual int SmallChange
    {
      get => _smallChange;
      set
      {
        if (_smallChange != value)
        {
          _smallChange = value;

          OnSmallChangeChanged(EventArgs.Empty);
        }
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get => base.Text;
      set => base.Text = value;
    }

    #endregion Public Properties

    #region Protected Properties

    /// <summary>
    ///   Gets a value indicating whether painting of the control is allowed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if painting of the control is allowed; otherwise, <c>false</c>.
    /// </value>
    protected virtual bool AllowPainting => _updateCount == 0;

    [Obsolete("Do not use. This property will be removed in a future update.")]
    protected Color[] Colors
    {
      get => _colors;
      set => _colors = value;
    }

    [Obsolete("Do not use. This property will be removed in a future update.")]
    protected bool LockUpdates
    {
      get => _lockUpdates;
      set => _lockUpdates = value;
    }

    [Obsolete("Do not use. This property will be removed in a future update.")]
    protected PointF[] Points
    {
      get => _points;
      set => _points = value;
    }

    [Obsolete("Do not use. This property will be removed in a future update.")]
    protected Image SelectionGlyph
    {
      get => _selectionGlyph;
      set => _selectionGlyph = value;
    }

    #endregion Protected Properties

    #region Public Methods

    /// <summary>
    ///   Disables any redrawing of the image box
    /// </summary>
    public virtual void BeginUpdate()
    {
      _updateCount++;
    }

    /// <summary>
    ///   Enables the redrawing of the image box
    /// </summary>
    public virtual void EndUpdate()
    {
      if (_updateCount > 0)
      {
        _updateCount--;
      }

      if (AllowPainting)
      {
        Invalidate();
      }
    }

    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    /// Calculates wheel attributes.
    /// </summary>
    protected virtual void CalculateWheel()
    {
      PointF[] points;
      Color[] colors;
      Size size;

      size = ClientSize;

      // Only define the points if the control is above a minimum size, otherwise if it's too small, you get an "out of memory" exceptions (of all things) when creating the brush
      if (size.Width > 16 && size.Height > 16 && _colorStep > 0)
      {
        int count;
        int w;
        int h;
        double l;
        double angle;

        count = 360 / _colorStep;
        points = new PointF[count];
        colors = new Color[count];
        angle = 0;

        w = size.Width;
        h = size.Height;
        l = _displayLightness
          ? _lightness
          : 0.5;

        _centerPoint = new PointF(w / 2.0F, h / 2.0F);
        _radius = GetRadius(_centerPoint);

        for (var i = 0; i < count; i++)
        {
          double angleR;
          PointF location;

          angleR = angle * (Math.PI / 180);
          location = GetColorLocation(angleR, _radius);

          points[i] = location;
          colors[i] = HslColor.HslToRgb(angle, 1, l);

          angle += _colorStep;
        }
      }
      else
      {
        points = null;
        colors = null;
      }

      _points = points;
      _colors = colors;
    }

    /// <summary>
    /// Creates the gradient brush used to paint the wheel.
    /// </summary>
    protected virtual Brush CreateGradientBrush()
    {
      Brush result;

      if (_points != null && _points.Length != 0 && _points.Length == _colors.Length)
      {
        result = new PathGradientBrush(_points, WrapMode.Clamp)
        {
          CenterPoint = _centerPoint,
          CenterColor = Color.White,
          SurroundColors = _colors
        };
      }
      else
      {
        result = null;
      }

      return result;
    }

    /// <summary>
    /// Creates the selection glyph.
    /// </summary>
    protected virtual Image CreateSelectionGlyph() => null;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" /> and its child controls and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _linePen?.Dispose();
        _linePen = null;

        DisposeOfWheelBrush();
        DisposeOfSelectionGlyph();
      }

      base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the point within the wheel representing the source color.
    /// </summary>
    /// <param name="color">The color.</param>
    protected PointF GetColorLocation(Color color)
    {
      return GetColorLocation(new HslColor(color));
    }

    /// <summary>
    /// Gets the point within the wheel representing the source color.
    /// </summary>
    /// <param name="color">The color.</param>
    protected virtual PointF GetColorLocation(HslColor color)
    {
      double angle;
      double radius;

      angle = GetHueAngle(color.H);
      radius = _radius * color.S;

      return GetColorLocation(angle, radius);
    }

    protected PointF GetColorLocation(double angleR, double radius)
    {
      Padding padding;
      double x;
      double y;

      padding = Padding;
      x = padding.Left + _centerPoint.X + Math.Cos(angleR) * radius;
      y = padding.Top + _centerPoint.Y - Math.Sin(angleR) * radius;

      return new PointF((float)x, (float)y);
    }

    protected float GetRadius(PointF centerPoint)
    {
      Padding padding;
      int offset;

      padding = Padding;
      offset = _showAngleArrow
        ? _selectionSize
        : _selectionSize / 2;

      return Math.Min(centerPoint.X, centerPoint.Y) - (Math.Max(padding.Horizontal, padding.Vertical) + offset);
    }

    /// <summary>
    /// Determines whether the specified key is a regular input key or a special key that requires preprocessing.
    /// </summary>
    /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values.</param>
    /// <returns>true if the specified key is a regular input key; otherwise, false.</returns>
    protected override bool IsInputKey(Keys keyData)
    {
      bool result;

      if ((keyData & Keys.Left) == Keys.Left || (keyData & Keys.Up) == Keys.Up || (keyData & Keys.Down) == Keys.Down ||
          (keyData & Keys.Right) == Keys.Right || (keyData & Keys.PageUp) == Keys.PageUp ||
          (keyData & Keys.PageDown) == Keys.PageDown || (keyData & Keys.Home) == Keys.Home || (keyData & Keys.End) == Keys.End)
      {
        result = true;
      }
      else
      {
        result = base.IsInputKey(keyData);
      }

      return result;
    }

    /// <summary>
    /// Determines whether the specified point is within the bounds of the color wheel.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns><c>true</c> if the specified point is within the bounds of the color wheel; otherwise, <c>false</c>.</returns>
    protected bool IsPointInWheel(Point point)
    {
      PointF normalized;

      // http://my.safaribooksonline.com/book/programming/csharp/9780672331985/graphics-with-windows-forms-and-gdiplus/ch17lev1sec21

      normalized = new PointF(point.X - _centerPoint.X, point.Y - _centerPoint.Y);

      return normalized.X * normalized.X + normalized.Y * normalized.Y <= _radius * _radius;
    }

    /// <summary>
    /// Raises the <see cref="AlphaChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnAlphaChanged(EventArgs e)
    {
      EventHandler handler;

      handler = (EventHandler)Events[_eventAlphaChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ColorChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnColorChanged(EventArgs e)
    {
      EventHandler handler;

      if (!_lockUpdates)
      {
        HslColor = new HslColor(_color);
      }

      Refresh();

      handler = (EventHandler)Events[_eventColorChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ColorStepChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnColorStepChanged(EventArgs e)
    {
      EventHandler handler;

      RefreshWheel();

      handler = (EventHandler)Events[_eventColorStepChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="DisplayLightnessChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnDisplayLightnessChanged(EventArgs e)
    {
      EventHandler handler;

      RefreshWheel();

      handler = (EventHandler)Events[_eventDisplayLightnessChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      base.OnGotFocus(e);

      Invalidate();
    }

    /// <summary>
    /// Raises the <see cref="HslColorChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnHslColorChanged(EventArgs e)
    {
      EventHandler handler;

      if (!_lockUpdates)
      {
        SetRgbColor(_hslColor);
      }

      Invalidate();

      handler = (EventHandler)Events[_eventHslColorChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (IsNavigationKey(e.KeyCode))
      {
        HslColor color;
        double hue;
        double saturation;
        int step;

        e.Handled = true;

        color = _hslColor;
        hue = color.H;
        saturation = color.S;

        step = e.Shift
          ? _largeChange
          : _smallChange;

        switch (e.KeyCode)
        {
          case Keys.Right:
            hue += step;
            break;

          case Keys.Up:
            saturation += step / 100F;
            break;

          case Keys.Left:
            hue -= step;
            break;

          case Keys.Down:
            saturation -= step / 100F;
            break;

          case Keys.PageUp:
            hue += _largeChange;
            break;

          case Keys.PageDown:
            hue -= _largeChange;
            break;

          case Keys.Home:
            saturation = 1;
            break;

          case Keys.End:
            saturation = 0;
            break;
        }

        if (hue >= 360)
        {
          hue = 0;
        }
        else if (hue < 0)
        {
          hue = 359;
        }

        if (saturation > 1)
        {
          saturation = 1;
        }
        else if (saturation < 0)
        {
          saturation = 0;
        }

        if (Math.Abs(hue - color.H) > double.Epsilon || Math.Abs(saturation - color.S) > double.Epsilon)
        {
          color.H = hue;
          color.S = saturation;

          // As the Color and HslColor properties update each other, need to temporarily disable this and manually set both
          // otherwise the wheel "sticks" due to imprecise conversion from RGB to HSL
          _lockUpdates = true;
          SetRgbColor(color);
          HslColor = color;
          _lockUpdates = false;
        }
      }

      base.OnKeyDown(e);
    }

    /// <summary>
    /// Raises the <see cref="LargeChangeChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnLargeChangeChanged(EventArgs e)
    {
      EventHandler handler;

      handler = (EventHandler)Events[_eventLargeChangeChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="LightnessChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnLightnessChanged(EventArgs e)
    {
      EventHandler handler;

      if (_displayLightness)
      {
        RefreshWheel();
      }

      Invalidate();

      handler = (EventHandler)Events[_eventLightnessChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="LineColorChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnLineColorChanged(EventArgs e)
    {
      EventHandler handler;

      Invalidate();

      handler = (EventHandler)Events[_eventLineColorChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      base.OnLostFocus(e);

      Invalidate();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);

      if (!Focused && TabStop)
      {
        Focus();
      }

      if (e.Button == MouseButtons.Left && IsPointInWheel(e.Location))
      {
        _isDragging = true;
        SetColor(e.Location);
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      if (_isDragging)
      {
        SetColor(e.Location);
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data. </param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);

      _isDragging = false;
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.PaddingChanged" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnPaddingChanged(EventArgs e)
    {
      base.OnPaddingChanged(e);

      RefreshWheel();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);

      if (AllowPainting)
      {
        Control parent;
        Graphics g;

        OnPaintBackground(e); // HACK: Easiest way of supporting things like BackgroundImage, BackgroundImageLayout etc

        // if the parent is using a transparent color, it's likely to be something like a TabPage in a tab control
        // so we'll draw the parent background instead, to avoid having an ugly solid color
        parent = Parent;
        if (BackgroundImage == null && parent != null && (BackColor == parent.BackColor || parent.BackColor.A != 255))
        {
          ButtonRenderer.DrawParentBackground(e.Graphics, DisplayRectangle, this);
        }

        g = e.Graphics;

        if (_brush != null)
        {
          g.FillEllipse(_brush, ClientRectangle);
        }

        g.SmoothingMode = SmoothingMode.AntiAlias;

        // HACK: smooth out the edge of the wheel.
        // https://github.com/cyotek/ AltUI.ColorPicker.ColorPicker/issues/1 - the linked source doesn't do this hack yet draws with a smoother edge
        using (var pen = CreateSmoothingPen())
        {
          g.DrawEllipse(pen, new RectangleF(_centerPoint.X - _radius, _centerPoint.Y - _radius, _radius * 2, _radius * 2));
        }

        PaintCenterLines(g);

        if (!_color.IsEmpty)
        {
          PaintSaturationRing(g);
          PaintArrowHead(g);
          PaintCustomColors(e);
          PaintCurrentColor(e);
        }
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.Resize" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      RefreshWheel();
    }

    /// <summary>
    /// Raises the <see cref="SecondarySelectionSizeChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnSecondarySelectionSizeChanged(EventArgs e)
    {
      EventHandler handler;

      Invalidate();

      handler = (EventHandler)Events[_eventSecondarySelectionSizeChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="SelectionSizeChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnSelectionSizeChanged(EventArgs e)
    {
      EventHandler handler;

      DisposeOfSelectionGlyph();

      RefreshWheel();

      handler = (EventHandler)Events[_eventSelectionSizeChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ShowAngleArrowChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnShowAngleArrowChanged(EventArgs e)
    {
      EventHandler handler;

      RefreshWheel();

      handler = (EventHandler)Events[_eventShowAngleArrowChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ShowCenterLinesChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnShowCenterLinesChanged(EventArgs e)
    {
      EventHandler handler;

      Invalidate();

      handler = (EventHandler)Events[_eventShowCenterLinesChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="ShowSaturationRingChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnShowSaturationRingChanged(EventArgs e)
    {
      EventHandler handler;

      Invalidate();

      handler = (EventHandler)Events[_eventShowSaturationRingChanged];

      handler?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="SmallChangeChanged" /> event.
    /// </summary>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected virtual void OnSmallChangeChanged(EventArgs e)
    {
      EventHandler handler;

      handler = (EventHandler)Events[_eventSmallChangeChanged];

      handler?.Invoke(this, e);
    }

    protected void PaintColor(PaintEventArgs e, HslColor color)
    {
      PaintColor(e, color, false);
    }

    protected virtual void PaintColor(PaintEventArgs e, HslColor color, bool includeFocus)
    {
      PointF location;

      location = GetColorLocation(color);

      if (!float.IsNaN(location.X) && !float.IsNaN(location.Y))
      {
        int size;
        int x;
        int y;

        size = includeFocus
          ? _selectionSize
          : _secondarySelectionSize;

        x = (int)location.X - size / 2;
        y = (int)location.Y - size / 2;

        if (_selectionGlyph == null)
        {
          using (Brush brush = new SolidBrush(color.ToRgbColor()))
          {
            e.Graphics.FillEllipse(brush, x, y, size, size);
          }

          e.Graphics.DrawEllipse(_linePen, x, y, size, size);
        }
        else
        {
          e.Graphics.DrawImage(_selectionGlyph, x, y);
        }

        if (Focused && includeFocus)
        {
          NativeMethods.DrawFocusRectangle(e.Graphics, new Rectangle(x - 2, y - 2, size + 5, size + 5));
        }
      }
    }

    protected virtual void PaintCurrentColor(PaintEventArgs e)
    {
      PaintColor(e, _hslColor, true);
    }

    protected virtual void SetColor(Point point)
    {
      double dx;
      double dy;
      double angle;
      double distance;
      double saturation;
      Padding padding;
      HslColor newColor;

      padding = Padding;
      dx = Math.Abs(point.X - _centerPoint.X - padding.Left);
      dy = Math.Abs(point.Y - _centerPoint.Y - padding.Top);
      angle = Math.Atan(dy / dx) / Math.PI * 180;
      distance = Math.Pow(Math.Pow(dx, 2) + Math.Pow(dy, 2), 0.5);
      saturation = distance / _radius;

      if (point.X < _centerPoint.X)
      {
        angle = 180 - angle;
      }

      if (point.Y > _centerPoint.Y)
      {
        angle = 360 - angle;
      }

      newColor = new HslColor(angle, saturation, _lightness);

      if (_hslColor != newColor)
      {
        _lockUpdates = true;
        HslColor = newColor;
        SetRgbColor(_hslColor);
        _lockUpdates = false;
      }
    }

    #endregion Protected Methods

    #region Private Methods

    private static HslColor[] GetEmptyColorArray()
    {
#if NET46_OR_GREATER
      return Array.Empty<HslColor>();
#else
      return _empty;
#endif
    }

    private void CreateLinePen()
    {
      _linePen?.Dispose();

      _linePen = new Pen(_lineColor);
    }

    private Pen CreateSmoothingPen()
    {
      Color color;

      color = BackColor;

      if (Parent is TabPage)
      {
        // HACK: Should probably try to get the
        // actual theme colour but perhaps Window
        // or White will do fine
        color = SystemColors.Window;
      }

      return new Pen(color, 2)
      {
        Alignment = PenAlignment.Outset
      };
    }

    private void DefineArrowHead()
    {
      _arrowHead = new[]
      {
        new Point(0,0),
        new Point(_selectionSize,0),
        new Point(0,_selectionSize)
      };
    }

    private void DisposeOfSelectionGlyph()
    {
      if (_selectionGlyph != null)
      {
        _selectionGlyph.Dispose();
        _selectionGlyph = null;
      }
    }

    private void DisposeOfWheelBrush()
    {
      if (_brush != null)
      {
        _brush.Dispose();
        _brush = null;
      }
    }

    private double GetHueAngle(double hue)
    {
      return hue * Math.PI / 180;
    }

    private bool IsNavigationKey(Keys keyCode)
    {
      return keyCode == Keys.Up
             || keyCode == Keys.Down
             || keyCode == Keys.Left
             || keyCode == Keys.Right
             || keyCode == Keys.PageUp
             || keyCode == Keys.PageDown
             || keyCode == Keys.Home
             || keyCode == Keys.End;
    }

    private void PaintArrowHead(Graphics g)
    {
      if (_showAngleArrow)
      {
        PointF head;

        head = GetColorLocation(GetHueAngle(_hslColor.H), _radius);

        g.TranslateTransform(head.X, head.Y);
        g.RotateTransform(-(float)(_hslColor.H + 45));
        g.FillPolygon(Brushes.White, _arrowHead);
        g.DrawPolygon(_linePen, _arrowHead);

        g.ResetTransform();
      }
    }

    private void PaintCenterLine(Graphics g, HslColor color, bool fullRadius)
    {
      PointF start;

      start = fullRadius
        ? GetColorLocation(GetHueAngle(color.H), _radius)
        : GetColorLocation(color);

      g.DrawLine(_linePen, start, _centerPoint);
    }

    private void PaintCenterLines(Graphics g)
    {
      if (_showCenterLines)
      {
        if (!_hslColor.IsEmpty)
        {
          PaintCenterLine(g, _hslColor, _showAngleArrow);
        }

        if (_secondaryColors != null && _secondaryColors.Length > 0)
        {
          for (var i = 0; i < _secondaryColors.Length; i++)
          {
            PaintCenterLine(g, _secondaryColors[i], false);
          }
        }
      }
    }

    private void PaintCustomColors(PaintEventArgs e)
    {
      if (_secondaryColors != null && _secondaryColors.Length > 0)
      {
        for (var i = 0; i < _secondaryColors.Length; i++)
        {
          PaintColor(e, _secondaryColors[i], false);
        }
      }
    }

    private void PaintSaturationRing(Graphics g)
    {
      if (_showSaturationRing)
      {
        float radius;

        radius = (float)(_radius * _hslColor.S);

        using Pen pen = new(HslColor.HslToRgb(0, 0, _hslColor.S));
        g.DrawEllipse(pen, new RectangleF(_centerPoint.X - radius, _centerPoint.Y - radius, radius * 2, radius * 2));
      }
    }

    /// <summary>
    /// Refreshes the wheel attributes and then repaints the control
    /// </summary>
    private void RefreshWheel()
    {
      CalculateWheel();

      DisposeOfWheelBrush();
      _brush = CreateGradientBrush();

      if (_selectionGlyph == null)
      {
        _selectionGlyph = CreateSelectionGlyph();
      }

      DefineArrowHead();

      Invalidate();
    }

    private void SetRgbColor(HslColor hslColor)
    {
      Color = hslColor.ToRgbColor(Convert.ToInt32(_alpha * 255));
    }

    #endregion Private Methods
  }
}
