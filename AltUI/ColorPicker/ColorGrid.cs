// Cyotek Color Picker Controls Library
// http://cyotek.com/blog/tag/colorpicker

// Copyright (c) 2013-2021 Cyotek Ltd.

// This work is licensed under the MIT License.
// See LICENSE.TXT for the full text

// Found this code useful?
// https://www.cyotek.com/contribute

using AltUI.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AltUI.Controls;

namespace AltUI.ColorPicker
{
    /// <summary>
    /// Represents a grid control, which displays a collection of colors using different styles.
    /// </summary>
    [DefaultProperty("Color")]
    [DefaultEvent("ColorChanged")]
    [ToolboxBitmap(typeof(ColorGrid), "ColorGridToolboxBitmap.bmp")]
    [ToolboxItem(true)]
    public class ColorGrid : Control, IColorEditor
    {
        #region Constants

        public const int InvalidIndex = -1;

        private readonly IDictionary<int, Rectangle> _colorRegions;

        private static readonly object _eventAutoAddColorsChanged = new object();

        private static readonly object _eventAutoFitChanged = new object();

        private static readonly object _eventCellBorderColorChanged = new object();

        private static readonly object _eventCellBorderStyleChanged = new object();

        private static readonly object _eventCellContextMenuStripChanged = new object();

        private static readonly object _eventCellSizeChanged = new object();

        private static readonly object _eventColorChanged = new object();

        private static readonly object _eventColorIndexChanged = new object();

        private static readonly object _eventColorsChanged = new object();

        private static readonly object _eventColumnsChanged = new object();

        private static readonly object _eventCustomColorsChanged = new object();

        private static readonly object _eventEditingColor = new object();

        private static readonly object _eventEditModeChanged = new object();

        private static readonly object _eventHotIndexChanged = new object();

        private static readonly object _eventPaletteChanged = new object();

        private static readonly object _eventSelectedCellStyleChanged = new object();

        private static readonly object _eventShowCustomColorsChanged = new object();

        private static readonly object _eventShowToolTipsChanged = new object();

        private static readonly object _eventSpacingChanged = new object();

        #endregion Constants

        #region Fields

        private bool _autoAddColors;

        private bool _autoFit;

        private Brush _cellBackgroundBrush;

        private Color _cellBorderColor;

        private ColorCellBorderStyle _cellBorderStyle;

        private ContextMenuStrip _cellContextMenuStrip;

        private Size _cellSize;

        private Color _color;

        private int _colorIndex;

        private ColorCollection _colors;

        private int _columns;

        private ColorCollection _customColors;

        private ColorEditingMode _editMode;

        private bool _layoutRequired;

        private int _hotIndex;

        private ColorPalette _palette;

        private int _previousColorIndex;

        private int _previousHotIndex;

        private ColorGridSelectedCellStyle _selectedCellStyle;

        private bool _showCustomColors;

        private bool _showToolTips;

        private Size _spacing;

        private DarkToolTip _toolTip;

        private int _updateCount;

        #endregion Fields

        #region Constructors

        public ColorGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick | ControlStyles.SupportsTransparentBackColor, true);
            _previousHotIndex = InvalidIndex;
            _previousColorIndex = InvalidIndex;
            _hotIndex = InvalidIndex;

            _colorRegions = new Dictionary<int, Rectangle>();
            _colors = ColorPalettes.NamedColors;
            _customColors = new ColorCollection(Enumerable.Repeat(Color.White, 16));
            _showCustomColors = true;
            _cellSize = new Size(12, 12);
            _spacing = new Size(3, 3);
            _columns = 16;
            base.AutoSize = true;
            Padding = new Padding(5);
            _autoAddColors = true;
            _cellBorderColor = SystemColors.ButtonShadow;
            _showToolTips = true;
            _toolTip = new DarkToolTip();
            SeparatorHeight = 8;
            _editMode = ColorEditingMode.CustomOnly;
            _color = Color.Black;
            _cellBorderStyle = ColorCellBorderStyle.FixedSingle;
            _selectedCellStyle = ColorGridSelectedCellStyle.Zoomed;
            _palette = ColorPalette.Named;

            AddEventHandlers(_colors);
            AddEventHandlers(_customColors);

            SetScaledCellSize();
            RefreshColors();
        }

        #endregion Constructors

        #region Events

        [Category("Property Changed")]
        public event EventHandler AutoAddColorsChanged
        {
            add => Events.AddHandler(_eventAutoAddColorsChanged, value);
            remove => Events.RemoveHandler(_eventAutoAddColorsChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler AutoFitChanged
        {
            add => Events.AddHandler(_eventAutoFitChanged, value);
            remove => Events.RemoveHandler(_eventAutoFitChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler CellBorderColorChanged
        {
            add => Events.AddHandler(_eventCellBorderColorChanged, value);
            remove => Events.RemoveHandler(_eventCellBorderColorChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler CellBorderStyleChanged
        {
            add => Events.AddHandler(_eventCellBorderStyleChanged, value);
            remove => Events.RemoveHandler(_eventCellBorderStyleChanged, value);
        }

        /// <summary>
        /// Occurs when the CellContextMenuStrip property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler CellContextMenuStripChanged
        {
            add => Events.AddHandler(_eventCellContextMenuStripChanged, value);
            remove => Events.RemoveHandler(_eventCellContextMenuStripChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler CellSizeChanged
        {
            add => Events.AddHandler(_eventCellSizeChanged, value);
            remove => Events.RemoveHandler(_eventCellSizeChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler ColorIndexChanged
        {
            add => Events.AddHandler(_eventColorIndexChanged, value);
            remove => Events.RemoveHandler(_eventColorIndexChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler ColorsChanged
        {
            add => Events.AddHandler(_eventColorsChanged, value);
            remove => Events.RemoveHandler(_eventColorsChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler ColumnsChanged
        {
            add => Events.AddHandler(_eventColumnsChanged, value);
            remove => Events.RemoveHandler(_eventColumnsChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler CustomColorsChanged
        {
            add => Events.AddHandler(_eventCustomColorsChanged, value);
            remove => Events.RemoveHandler(_eventCustomColorsChanged, value);
        }

        [Category("Action")]
        public event EventHandler<EditColorCancelEventArgs> EditingColor
        {
            add => Events.AddHandler(_eventEditingColor, value);
            remove => Events.RemoveHandler(_eventEditingColor, value);
        }

        [Category("Property Changed")]
        public event EventHandler EditModeChanged
        {
            add => Events.AddHandler(_eventEditModeChanged, value);
            remove => Events.RemoveHandler(_eventEditModeChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler HotIndexChanged
        {
            add => Events.AddHandler(_eventHotIndexChanged, value);
            remove => Events.RemoveHandler(_eventHotIndexChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler PaletteChanged
        {
            add => Events.AddHandler(_eventPaletteChanged, value);
            remove => Events.RemoveHandler(_eventPaletteChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler SelectedCellStyleChanged
        {
            add => Events.AddHandler(_eventSelectedCellStyleChanged, value);
            remove => Events.RemoveHandler(_eventSelectedCellStyleChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler ShowCustomColorsChanged
        {
            add => Events.AddHandler(_eventShowCustomColorsChanged, value);
            remove => Events.RemoveHandler(_eventShowCustomColorsChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler ShowToolTipsChanged
        {
            add => Events.AddHandler(_eventShowToolTipsChanged, value);
            remove => Events.RemoveHandler(_eventShowToolTipsChanged, value);
        }

        [Category("Property Changed")]
        public event EventHandler SpacingChanged
        {
            add => Events.AddHandler(_eventSpacingChanged, value);
            remove => Events.RemoveHandler(_eventSpacingChanged, value);
        }

        #endregion Events

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ActualColumns { get; protected set; }

        [Category("Behavior")]
        [DefaultValue(true)]
        public virtual bool AutoAddColors
        {
            get => _autoAddColors;
            set
            {
                if (AutoAddColors != value)
                {
                    _autoAddColors = value;

                    OnAutoAddColorsChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public virtual bool AutoFit
        {
            get => _autoFit;
            set
            {
                if (AutoFit == value) return;
                _autoFit = value;

                OnAutoFitChanged(EventArgs.Empty);
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "ButtonShadow")]
        public virtual Color CellBorderColor
        {
            get => _cellBorderColor;
            set
            {
                if (CellBorderColor == value) return;
                _cellBorderColor = value;

                OnCellBorderColorChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(ColorCellBorderStyle), "FixedSingle")]
        public virtual ColorCellBorderStyle CellBorderStyle
        {
            get => _cellBorderStyle;
            set
            {
                if (CellBorderStyle != value)
                {
                    _cellBorderStyle = value;

                    OnCellBorderStyleChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(typeof(ContextMenuStrip), null)]
        public ContextMenuStrip CellContextMenuStrip
        {
            get => _cellContextMenuStrip;
            set
            {
                if (_cellContextMenuStrip == value) return;
                _cellContextMenuStrip = value;

                OnCellContextMenuStripChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Size), "12, 12")]
        public virtual Size CellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;

                OnCellSizeChanged(EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int ColorIndex
        {
            get => _colorIndex;
            set
            {
                if (ColorIndex == value) return;
                _previousColorIndex = _colorIndex;
                _colorIndex = value;

                if (value != InvalidIndex)
                {
                    Color = GetColor(value);
                }

                OnColorIndexChanged(EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ColorCollection Colors
        {
            get => _colors;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (Colors != value)
                {
                    RemoveEventHandlers(_colors);

                    _colors = value;

                    OnColorsChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(16)]
        public virtual int Columns
        {
            get => _columns;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Number of columns cannot be less than zero.");
                }

                if (Columns != value)
                {
                    _columns = value;
                    CalculateGridSize();

                    OnColumnsChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public Point CurrentCell => GetCell(ColorIndex);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ColorCollection CustomColors
        {
            get => _customColors;
            set
            {
                if (CustomColors != value)
                {
                    RemoveEventHandlers(_customColors);

                    _customColors = value;

                    OnCustomColorsChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(typeof(ColorEditingMode), "CustomOnly")]
        public virtual ColorEditingMode EditMode
        {
            get => _editMode;
            set
            {
                if (EditMode != value)
                {
                    _editMode = value;

                    OnEditModeChanged(EventArgs.Empty);
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int HotIndex
        {
            get => _hotIndex;
            set
            {
                if (HotIndex != value)
                {
                    _previousHotIndex = HotIndex;
                    _hotIndex = value;

                    OnHotIndexChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(typeof(Padding), "5, 5, 5, 5")]
        public new Padding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        [Category("Appearance")]
        [DefaultValue(typeof(ColorPalette), "Named")]
        public virtual ColorPalette Palette
        {
            get => _palette;
            set
            {
                if (Palette != value)
                {
                    _palette = value;

                    OnPaletteChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(ColorGridSelectedCellStyle), "Zoomed")]
        public virtual ColorGridSelectedCellStyle SelectedCellStyle
        {
            get => _selectedCellStyle;
            set
            {
                if (SelectedCellStyle != value)
                {
                    _selectedCellStyle = value;

                    OnSelectedCellStyleChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public virtual bool ShowCustomColors
        {
            get => _showCustomColors;
            set
            {
                if (ShowCustomColors != value)
                {
                    _showCustomColors = value;

                    OnShowCustomColorsChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public virtual bool ShowToolTips
        {
            get => _showToolTips;
            set
            {
                if (ShowToolTips != value)
                {
                    _showToolTips = value;

                    OnShowToolTipsChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Size), "3, 3")]
        public virtual Size Spacing
        {
            get => _spacing;
            set
            {
                if (Spacing != value)
                {
                    _spacing = value;

                    OnSpacingChanged(EventArgs.Empty);
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

        /// <summary>
        ///   Gets a value indicating whether painting of the control is allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if painting of the control is allowed; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool AllowPainting => _updateCount == 0;

        protected IDictionary<int, Rectangle> ColorRegions => _colorRegions;

        protected int CustomRows { get; set; }

        protected int PrimaryRows { get; set; }

        protected int SeparatorHeight { get; set; }

        protected bool WasKeyPressed { get; set; }

        #endregion Properties

        #region Methods

        public virtual int AddCustomColor(Color value)
        {
            var newIndex = GetColorIndex(value);

            if (newIndex == InvalidIndex)
            {
                if (AutoAddColors)
                {
                    CustomColors.Add(value);
                }
                else
                {
                    if (CustomColors == null)
                    {
                        CustomColors = new ColorCollection();
                        CustomColors.Add(value);
                    }
                    else
                    {
                        CustomColors[0] = value;
                    }

                    newIndex = GetColorIndex(value);
                }

                if (_showCustomColors)
                {
                    RefreshColors();
                }
            }

            return newIndex;
        }

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
                if (_layoutRequired)
                {
                    RefreshColors();
                    _layoutRequired = false;
                }
                else
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="Rectangle"/> describing the bounds of a single color cell
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the
        /// required range.</exception>
        /// <param name="index">Zero-based index of the color cell to return.</param>
        /// <returns>
        /// The cell bounds.
        /// </returns>
        public Rectangle GetCellBounds(int index)
        {
            if (index < 0 || index > Colors.Count + CustomColors.Count - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _colorRegions[index];
        }

        public Color GetColor(int index)
        {
            Color result;

            var colorCount = Colors != null ? Colors.Count : 0;
            var customColorCount = CustomColors != null ? CustomColors.Count : 0;

            if (index < 0 || index > colorCount + customColorCount)
            {
                result = Color.Empty;
            }
            else
            {
                result = index > colorCount - 1 ? CustomColors[index - colorCount] : Colors[index];
            }

            return result;
        }

        public ColorSource GetColorSource(int colorIndex)
        {
            ColorSource result;

            var colorCount = Colors != null ? Colors.Count : 0;
            var customColorCount = CustomColors != null ? CustomColors.Count : 0;

            if (colorCount < 0 || colorIndex > colorCount + customColorCount)
            {
                result = ColorSource.None;
            }
            else
            {
                result = colorIndex > colorCount - 1 ? ColorSource.Custom : ColorSource.Standard;
            }

            return result;
        }

        public ColorSource GetColorSource(Color color)
        {
            ColorSource result;

            var index = Colors.IndexOf(color);
            if (index != InvalidIndex)
            {
                result = ColorSource.Standard;
            }
            else
            {
                index = CustomColors.IndexOf(color);
                result = index != InvalidIndex ? ColorSource.Custom : ColorSource.None;
            }

            return result;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return AutoSize ? GetAutoSize() : base.GetPreferredSize(proposedSize);
        }

        public ColorHitTestInfo HitTest(Point point)
        {
            var result = new ColorHitTestInfo();
            var colorIndex = InvalidIndex;

            foreach (var pair in _colorRegions.Where(pair => pair.Value.Contains(point)))
            {
                colorIndex = pair.Key;
                break;
            }

            result.Index = colorIndex;
            if (colorIndex != InvalidIndex)
            {
                result.Color = colorIndex < Colors.Count + CustomColors.Count ? GetColor(colorIndex) : Color.White;
                result.Source = GetColorSource(colorIndex);
            }
            else
            {
                result.Source = ColorSource.None;
            }

            return result;
        }

        public void Invalidate(int index)
        {
            if (AllowPainting && index != InvalidIndex)
            {
                if (_colorRegions.TryGetValue(index, out var bounds))
                {
                    if (SelectedCellStyle == ColorGridSelectedCellStyle.Zoomed)
                    {
                        bounds.Inflate(Padding.Left, Padding.Top);
                    }

                    Invalidate(bounds);
                }
            }
        }

        public void Navigate(int offsetX, int offsetY)
        {
            Navigate(offsetX, offsetY, NavigationOrigin.Current);
        }

        public virtual void Navigate(int offsetX, int offsetY, NavigationOrigin origin)
        {
            Point cellLocation;

            switch (origin)
            {
                case NavigationOrigin.Begin:
                    cellLocation = Point.Empty;
                    break;

                case NavigationOrigin.End:
                    cellLocation = new Point(ActualColumns - 1, PrimaryRows + CustomRows - 1);
                    break;

                default:
                    cellLocation = CurrentCell;
                    break;
            }

            if (cellLocation.X == -1 && cellLocation.Y == -1)
            {
                cellLocation = Point.Empty; // If no cell is selected, assume the first one is for the purpose of keyboard navigation
            }

            var offsetCellLocation = GetCellOffset(cellLocation, offsetX, offsetY);
            var row = offsetCellLocation.Y;
            var column = offsetCellLocation.X;
            var index = GetCellIndex(column, row);
            if (index != InvalidIndex)
            {
                ColorIndex = index;
            }
        }

        protected virtual void CalculateCellSize()
        {
            var w = (ClientSize.Width - Padding.Horizontal) / ActualColumns - Spacing.Width;
            var h = (ClientSize.Height - Padding.Vertical) / (PrimaryRows + CustomRows) - Spacing.Height;

            if (w > 0 && h > 0)
            {
                CellSize = new Size(w, h);
            }
        }

        protected virtual void CalculateGridSize()
        {
            ActualColumns = Columns != 0 ? Columns : (ClientSize.Width + Spacing.Width - Padding.Vertical) / (_scaledCellSize.Width + Spacing.Width);
            if (ActualColumns < 1)
            {
                ActualColumns = 1;
            }

            var primaryRows = GetRows(Colors != null ? Colors.Count : 0);
            if (primaryRows == 0)
            {
                primaryRows = 1;
            }

            var customRows = ShowCustomColors ? GetRows(CustomColors != null ? CustomColors.Count : 0) : 0;

            PrimaryRows = primaryRows;
            CustomRows = customRows;
        }

        protected virtual Brush CreateTransparencyBrush()
        {
            return new TextureBrush(ResourceManager.CellBackground, WrapMode.Tile);
        }

        protected void DefineColorRegions(ColorCollection colors, int rangeStart, int offset)
        {
            if (colors != null)
            {
                var rows = GetRows(colors.Count);
                var index = 0;

                for (var row = 0; row < rows; row++)
                {
                    for (var column = 0; column < ActualColumns; column++)
                    {
                        if (index < colors.Count)
                        {
                            _colorRegions.Add(rangeStart + index, new Rectangle(Padding.Left + column * (_scaledCellSize.Width + Spacing.Width), offset + row * (_scaledCellSize.Height + Spacing.Height), _scaledCellSize.Width, _scaledCellSize.Height));
                        }

                        index++;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveEventHandlers(_colors);
                RemoveEventHandlers(_customColors);

                _toolTip?.Dispose();

                _cellBackgroundBrush?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected virtual void EditColor(int colorIndex)
        {
            using var dialog = new ColorPickerDialog();
            dialog.Color = GetColor(colorIndex);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                BeginUpdate();
                SetColor(colorIndex, dialog.Color);
                Color = dialog.Color;
                EndUpdate();
            }
        }

        protected Size GetAutoSize()
        {
            int width;

            var offset = CustomRows != 0 ? SeparatorHeight : 0;
            if (Columns != 0)
            {
                width = (_scaledCellSize.Width + Spacing.Width) * ActualColumns + Padding.Horizontal - Spacing.Width;
            }
            else
            {
                width = ClientSize.Width;
            }

            return new Size(width, (_scaledCellSize.Height + Spacing.Height) * (PrimaryRows + CustomRows) + offset + Padding.Vertical - Spacing.Height);
        }

        protected int GetCellIndex(Point point)
        {
            return GetCellIndex(point.X, point.Y);
        }

        protected virtual int GetCellIndex(int column, int row)
        {
            int result;

            if (column >= 0 && column < ActualColumns && row >= 0 && row < PrimaryRows + CustomRows)
            {
                var lastStandardRowOffset = PrimaryRows * ActualColumns - Colors.Count;
                result = row * ActualColumns + column;
                if (row == PrimaryRows - 1 && column >= ActualColumns - lastStandardRowOffset)
                {
                    result -= lastStandardRowOffset;
                }
                if (row >= PrimaryRows)
                {
                    result -= lastStandardRowOffset;
                }

                if (result > Colors.Count + CustomColors.Count - 1)
                {
                    result = InvalidIndex;
                }
            }
            else
            {
                result = InvalidIndex;
            }

            return result;
        }

        protected Point GetCellOffset(int columnOffset, int rowOffset)
        {
            return GetCellOffset(CurrentCell, columnOffset, rowOffset);
        }

        protected Point GetCellOffset(Point cell, int columnOffset, int rowOffset)
        {
            var lastStandardRowOffset = PrimaryRows * ActualColumns - Colors.Count;
            var lastStandardRowLastColumn = ActualColumns - lastStandardRowOffset;
            var column = cell.X + columnOffset;
            var row = cell.Y + rowOffset;

            // if the row is the last row, but there aren't enough columns to fill the row - nudge it to the last available
            if (row == PrimaryRows - 1 && column >= lastStandardRowLastColumn)
            {
                column = lastStandardRowLastColumn - 1;
            }

            // wrap the column to the end of the previous row
            if (column < 0)
            {
                column = ActualColumns - 1;
                row--;
                if (row == PrimaryRows - 1)
                {
                    column = ActualColumns - (lastStandardRowOffset + 1);
                }
            }

            // wrap to column to the start of the next row
            if (row == PrimaryRows - 1 && column >= ActualColumns - lastStandardRowOffset || column >= ActualColumns)
            {
                column = 0;
                row++;
            }

            return new Point(column, row);
        }

        protected virtual int GetColorIndex(Color value)
        {
            var index = Colors != null ? Colors.IndexOf(value) : InvalidIndex;
            if (index == InvalidIndex && ShowCustomColors && CustomColors != null)
            {
                index = CustomColors.IndexOf(value);
                if (index != InvalidIndex)
                {
                    index += Colors.Count;
                }
            }

            return index;
        }

        protected virtual ColorCollection GetPredefinedPalette()
        {
            return ColorPalettes.GetPalette(Palette);
        }

        protected int GetRows(int count)
        {
            int rows;

            if (count != 0 && ActualColumns > 0)
            {
                rows = count / ActualColumns;
                if (count % ActualColumns != 0)
                {
                    rows++;
                }
            }
            else
            {
                rows = 0;
            }

            return rows;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            bool result;

            if (keyData is Keys.Left or Keys.Up or Keys.Down or Keys.Right or Keys.Enter or Keys.Home or Keys.End)
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
        /// Raises the <see cref="AutoAddColorsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnAutoAddColorsChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[_eventAutoAddColorsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="AutoFitChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnAutoFitChanged(EventArgs e)
        {
            if (AutoFit && AutoSize)
            {
                AutoSize = false;
            }

            RefreshColors();

            var handler = (EventHandler)Events[_eventAutoFitChanged];

            handler?.Invoke(this, e);
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            if (AutoSize && AutoFit)
            {
                AutoFit = false;
            }

            base.OnAutoSizeChanged(e);

            if (AutoSize)
            {
                SizeToFit();
            }
        }

        /// <summary>
        /// Raises the <see cref="CellBorderColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnCellBorderColorChanged(EventArgs e)
        {
            if (AllowPainting)
            {
                Invalidate();
            }

            var handler = (EventHandler)Events[_eventCellBorderColorChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CellBorderStyleChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnCellBorderStyleChanged(EventArgs e)
        {
            if (AllowPainting)
            {
                Invalidate();
            }

            var handler = (EventHandler)Events[_eventCellBorderStyleChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CellContextMenuStripChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnCellContextMenuStripChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[_eventCellContextMenuStripChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CellSizeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnCellSizeChanged(EventArgs e)
        {
            SetScaledCellSize();

            if (AutoSize)
            {
                SizeToFit();
            }

            if (AllowPainting)
            {
                RefreshColors();
                Invalidate();
            }

            var handler = (EventHandler)Events[_eventCellSizeChanged];

            handler?.Invoke(this, e);
        }

        private void SetScaledCellSize()
        {
            var dpi = NativeMethods.GetDesktopDpi();
            var scaleX = dpi.X / 96F;
            var scaleY = dpi.Y / 96F;

            if (scaleX > 1 && scaleY > 1)
            {
                _scaledCellSize = new Size((int)(_cellSize.Width * scaleX), (int)(_cellSize.Height * scaleY));
            }
            else
            {
                _scaledCellSize = _cellSize;
            }
        }

        /// <summary>
        /// Raises the <see cref="ColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnColorChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[_eventColorChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ColorIndexChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnColorIndexChanged(EventArgs e)
        {
            if (AllowPainting)
            {
                Invalidate(_previousColorIndex);
                Invalidate(ColorIndex);
            }

            var handler = (EventHandler)Events[_eventColorIndexChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ColorsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnColorsChanged(EventArgs e)
        {
            AddEventHandlers(Colors);

            RefreshColors();

            var handler = (EventHandler)Events[_eventColorsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ColumnsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnColumnsChanged(EventArgs e)
        {
            RefreshColors();

            var handler = (EventHandler)Events[_eventColumnsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CustomColorsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnCustomColorsChanged(EventArgs e)
        {
            AddEventHandlers(CustomColors);
            RefreshColors();

            var handler = (EventHandler)Events[_eventCustomColorsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="EditingColor" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EditColorCancelEventArgs" /> instance containing the event data.</param>
        protected virtual void OnEditingColor(EditColorCancelEventArgs e)
        {
            var handler = (EventHandler<EditColorCancelEventArgs>)Events[_eventEditingColor];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="EditModeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnEditModeChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[_eventEditModeChanged];

            handler?.Invoke(this, e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (AllowPainting)
            {
                Invalidate(ColorIndex);
            }
        }

        /// <summary>
        /// Raises the <see cref="HotIndexChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnHotIndexChanged(EventArgs e)
        {
            SetToolTip();

            if (AllowPainting)
            {
                Invalidate(_previousHotIndex);
                Invalidate(HotIndex);
            }

            var handler = (EventHandler)Events[_eventHotIndexChanged];

            handler?.Invoke(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            WasKeyPressed = true;

            switch (e.KeyData)
            {
                case Keys.Down:
                    Navigate(0, 1);
                    e.Handled = true;
                    break;

                case Keys.Up:
                    Navigate(0, -1);
                    e.Handled = true;
                    break;

                case Keys.Left:
                    Navigate(-1, 0);
                    e.Handled = true;
                    break;

                case Keys.Right:
                    Navigate(1, 0);
                    e.Handled = true;
                    break;

                case Keys.Home:
                    Navigate(0, 0, NavigationOrigin.Begin);
                    e.Handled = true;
                    break;

                case Keys.End:
                    Navigate(0, 0, NavigationOrigin.End);
                    e.Handled = true;
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (WasKeyPressed && ColorIndex != InvalidIndex)
            {
                switch (e.KeyData)
                {
                    case Keys.Enter:

                        var source = GetColorSource(ColorIndex);

                        if (source == ColorSource.Custom && EditMode != ColorEditingMode.None || source == ColorSource.Standard && EditMode == ColorEditingMode.Both)
                        {
                            e.Handled = true;

                            StartColorEdit(ColorIndex);
                        }
                        break;

                    case Keys.Apps:
                    case Keys.F10 | Keys.Shift:

                        var location = _colorRegions[_colorIndex].Location;
                        var x = location.X;
                        var y = location.Y + _cellSize.Height;

                        ShowContextMenu(new Point(x, y));
                        break;
                }
            }

            WasKeyPressed = false;

            base.OnKeyUp(e);
        }

        private Size _scaledCellSize;

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (AllowPainting)
            {
                Invalidate(ColorIndex);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            var hitTest = HitTest(e.Location);

            if (e.Button == MouseButtons.Left && (hitTest.Source == ColorSource.Custom && EditMode != ColorEditingMode.None || hitTest.Source == ColorSource.Standard && EditMode == ColorEditingMode.Both))
            {
                StartColorEdit(hitTest.Index);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Focused && TabStop)
            {
                Focus();
            }

            ProcessMouseClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            HotIndex = InvalidIndex;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            HotIndex = HitTest(e.Location).Index;

            ProcessMouseClick(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                var index = HitTest(e.Location).Index;

                if (index != InvalidIndex)
                {
                    Focus();
                    ColorIndex = index;

                    ShowContextMenu(e.Location);
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);

            if (AllowPainting)
            {
                RefreshColors();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (AllowPainting)
            {
                var colorCount = Colors.Count;

                OnPaintBackground(e); // HACK: Easiest way of supporting things like BackgroundImage, BackgroundImageLayout etc as the PaintBackground event is no longer being called

                // draw a design time dotted grid
                if (DesignMode)
                {
                    using var pen = new Pen(SystemColors.ButtonShadow)
                    {
                        DashStyle = DashStyle.Dot
                    };
                    e.Graphics.DrawRoundedRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1), 4);
                }

                // draw cells for all current colors
                for (var i = 0; i < colorCount; i++)
                {
                    var bounds = _colorRegions[i];
                    if (e.ClipRectangle.IntersectsWith(bounds))
                    {
                        PaintCell(e, i, i, Colors[i], bounds);
                    }
                }

                if (CustomColors.Count != 0 && ShowCustomColors)
                {
                    // draw a separator
                    PaintSeparator(e);

                    // and the custom colors
                    for (var i = 0; i < CustomColors.Count; i++)
                    {
                        if (_colorRegions.TryGetValue(colorCount + i, out var bounds) && e.ClipRectangle.IntersectsWith(bounds))
                        {
                            PaintCell(e, i, colorCount + i, CustomColors[i], bounds);
                        }
                    }
                }

                // draw the selected color
                if (SelectedCellStyle == ColorGridSelectedCellStyle.None || ColorIndex < 0) return;
                {
                    if (_colorRegions.TryGetValue(ColorIndex, out var bounds) && e.ClipRectangle.IntersectsWith(bounds))
                    {
                        PaintSelectedCell(e, ColorIndex, Color, bounds);
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="PaletteChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnPaletteChanged(EventArgs e)
        {
            Colors = GetPredefinedPalette();

            var handler = (EventHandler)Events[_eventPaletteChanged];

            handler?.Invoke(this, e);
        }

        protected override void OnResize(EventArgs e)
        {
            RefreshColors();

            base.OnResize(e);
        }

        /// <summary>
        /// Raises the <see cref="SelectedCellStyleChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectedCellStyleChanged(EventArgs e)
        {
            if (AllowPainting)
            {
                Invalidate();
            }

            var handler = (EventHandler)Events[_eventSelectedCellStyleChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ShowCustomColorsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnShowCustomColorsChanged(EventArgs e)
        {
            RefreshColors();

            var handler = (EventHandler)Events[_eventShowCustomColorsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ShowToolTipsChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnShowToolTipsChanged(EventArgs e)
        {
            if (ShowToolTips)
            {
                _toolTip = new DarkToolTip();
            }
            else if (_toolTip != null)
            {
                _toolTip.Dispose();
                _toolTip = null;
            }

            var handler = (EventHandler)Events[_eventShowToolTipsChanged];

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SpacingChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSpacingChanged(EventArgs e)
        {
            if (AutoSize)
            {
                SizeToFit();
            }

            if (AllowPainting)
            {
                RefreshColors();
                Invalidate();
            }

            var handler = (EventHandler)Events[_eventSpacingChanged];

            handler?.Invoke(this, e);
        }

        protected virtual void PaintCell(PaintEventArgs e, int colorIndex, int cellIndex, Color color, Rectangle bounds)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (color.A != 255)
            {
                PaintTransparentCell(e, bounds);
            }

            using (Brush brush = new SolidBrush(color))
            {
                var rect = bounds with { Width = bounds.Width - 1, Height = bounds.Height - 1 };
                e.Graphics.FillRoundedRectangle(brush, rect, 4);
            }

            switch (CellBorderStyle)
            {
                case ColorCellBorderStyle.FixedSingle:
                    using (var pen = new Pen(CellBorderColor))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, new Rectangle(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1), 4);
                    }
                    break;

                case ColorCellBorderStyle.DoubleSoft:

                    var shadedOuter = new HslColor(color);
                    shadedOuter.L -= 0.50;

                    var shadedInner = new HslColor(color);
                    shadedInner.L -= 0.20;

                    using (var pen = new Pen(CellBorderColor))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, new Rectangle(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1), 4);
                    }
                    e.Graphics.DrawRoundedRectangle(Pens.White, new Rectangle(bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3), 4);
                    using (var pen = new Pen(Color.FromArgb(32, shadedOuter.ToRgbColor())))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, new Rectangle(bounds.Left + 2, bounds.Top + 2, bounds.Width - 5, bounds.Height - 5), 4);
                    }
                    using (var pen = new Pen(Color.FromArgb(32, shadedInner.ToRgbColor())))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, new Rectangle(bounds.Left + 3, bounds.Top + 3, bounds.Width - 7, bounds.Height - 7), 4);
                    }
                    break;
            }

            if (HotIndex != InvalidIndex && HotIndex == cellIndex)
            {
                e.Graphics.DrawRoundedRectangle(Pens.Black, new Rectangle(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1), 4);
                e.Graphics.DrawRoundedRectangle(Pens.White, new Rectangle(bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3), 4);
            }
        }

        protected virtual void PaintSelectedCell(PaintEventArgs e, int colorIndex, Color color, Rectangle bounds)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            switch (SelectedCellStyle)
            {
                case ColorGridSelectedCellStyle.Standard:
                    if (!Focused)
                    {
                        e.Graphics.DrawRoundedRectangle(Pens.Black, new Rectangle(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1), 4);
                    }
                    break;

                case ColorGridSelectedCellStyle.Zoomed:
                    // make the cell larger according to the padding
                    if (SelectedCellStyle == ColorGridSelectedCellStyle.Zoomed)
                    {
                        bounds.Inflate(Padding.Left, Padding.Top);
                    }

                    // fill the inner
                    var rect = bounds with { Width = bounds.Width - 1, Height = bounds.Height - 1 };
                    e.Graphics.FillRoundedRectangle(Brushes.White, rect, 4);
                    if (SelectedCellStyle == ColorGridSelectedCellStyle.Zoomed)
                    {
                        bounds.Inflate(-3, -3);
                    }
                    if (color.A != 255)
                    {
                        PaintTransparentCell(e, bounds);
                    }

                    using (Brush brush = new SolidBrush(color))
                    {
                        rect = bounds with { Width = bounds.Width - 1, Height = bounds.Height - 1 };
                        e.Graphics.FillRoundedRectangle(brush, rect, 4);
                    }

                    // draw a border
                    if (!Focused)
                    {
                        bounds = new Rectangle(bounds.Left - 2, bounds.Top - 2, bounds.Width + 3, bounds.Height + 3);

                        using var pen = new Pen(CellBorderColor);
                        e.Graphics.DrawRoundedRectangle(pen, bounds, 4);
                    }
                    break;
            }
        }

        protected virtual void PaintSeparator(PaintEventArgs e)
        {
            var x1 = Padding.Left;
            var x2 = ClientSize.Width - Padding.Right;
            var y1 = SeparatorHeight / 2 + Padding.Top + PrimaryRows * (_scaledCellSize.Height + Spacing.Height) + 1 - Spacing.Height;
            var y2 = y1;

            using var pen = new Pen(CellBorderColor);
            e.Graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        protected virtual void PaintTransparentCell(PaintEventArgs e, Rectangle bounds)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _cellBackgroundBrush ??= CreateTransparencyBrush();
            var rect = bounds with { Width = bounds.Width - 1, Height = bounds.Height - 1 };
            e.Graphics.FillRoundedRectangle(_cellBackgroundBrush, rect, 4);
        }

        protected virtual void ProcessMouseClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var hitTest = HitTest(e.Location);

            if (hitTest.Source == ColorSource.None) return;
            Color = hitTest.Color;
            ColorIndex = hitTest.Index;
        }

        protected virtual void RefreshColors()
        {
            if (AllowPainting)
            {
                CalculateGridSize();
                if (AutoFit)
                {
                    CalculateCellSize();
                }
                else if (AutoSize)
                {
                    SizeToFit();
                }

                _colorRegions.Clear();

                if (Colors == null) return;
                DefineColorRegions(Colors, 0, Padding.Top);
                if (ShowCustomColors)
                {
                    DefineColorRegions(CustomColors, Colors.Count, Padding.Top + SeparatorHeight + (_scaledCellSize.Height + Spacing.Height) * PrimaryRows);
                }

                ColorIndex = GetColorIndex(Color);

                if (!Color.IsEmpty && ColorIndex == InvalidIndex && AutoAddColors && ShowCustomColors)
                {
                    AddCustomColor(Color);
                }

                Invalidate();
            }
            else
            {
                _layoutRequired = true;
            }
        }

        protected virtual void SetColor(int colorIndex, Color color)
        {
            var colorCount = Colors.Count;

            if (colorIndex < 0 || colorIndex > colorCount + CustomColors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(colorIndex));
            }

            if (colorIndex > colorCount - 1)
            {
                CustomColors[colorIndex - colorCount] = color;
            }
            else
            {
                Colors[colorIndex] = color;
            }
        }

        private void AddEventHandlers(ColorCollection value)
        {
            if (value == null) return;
            value.ItemInserted += ColorsCollectionChangedHandler;
            value.ItemRemoved += ColorsCollectionChangedHandler;
            value.ItemsCleared += ColorsCollectionChangedHandler;
            value.ItemReplaced += ColorsCollectionItemReplacedHandler;
        }

        private void ColorsCollectionChangedHandler(object sender, ColorCollectionEventArgs e)
        {
            RefreshColors();
        }

        private void ColorsCollectionItemReplacedHandler(object sender, ColorCollectionEventArgs e)
        {
            var collection = (ColorCollection)sender;
            var index = _colorIndex;
            if (index != InvalidIndex && ReferenceEquals(collection, CustomColors))
            {
                index -= Colors.Count;
            }

            if (index >= 0 && index < collection.Count && collection[index] != Color)
            {
                _previousColorIndex = index;
                _colorIndex = -1;
                ColorIndex = index;
            }

            Invalidate(e.Index);
        }

        private Point GetCell(int index)
        {
            int row;
            int column;

            if (index == InvalidIndex)
            {
                row = -1;
                column = -1;
            }
            else if (index >= Colors.Count)
            {
                // custom color
                index -= Colors.Count;
                row = index / ActualColumns;
                column = index - row * ActualColumns;
                row += PrimaryRows;
            }
            else
            {
                // normal row
                row = index / ActualColumns;
                column = index - row * ActualColumns;
            }

            return new Point(column, row);
        }

        private void RemoveEventHandlers(ColorCollection value)
        {
            if (value != null)
            {
                value.ItemInserted -= ColorsCollectionChangedHandler;
                value.ItemRemoved -= ColorsCollectionChangedHandler;
                value.ItemsCleared -= ColorsCollectionChangedHandler;
                value.ItemReplaced -= ColorsCollectionItemReplacedHandler;
            }
        }

        private void SetToolTip()
        {
            if (!ShowToolTips) return;
#if USENAMEHACK
        string name;

        if (this.HotIndex != InvalidIndex)
        {
          name = this.HotIndex < this.Colors.Count ? this.Colors.GetName(this.HotIndex) : this.CustomColors.GetName(this.HotIndex);

          if (string.IsNullOrEmpty(name))
          {
            name = this.GetColor(this.HotIndex).Name;
          }
        }
        else
        {
          name = null;
        }

        _toolTip.SetToolTip(this, name);
#else
            _toolTip.SetToolTip(this, HotIndex != InvalidIndex ? GetColor(HotIndex).Name : null);
#endif
        }

        private void ShowContextMenu(Point location)
        {
            _cellContextMenuStrip?.Show(this, location);
        }

        private void SizeToFit()
        {
            Size = GetAutoSize();
        }

        private void StartColorEdit(int index)
        {
            var e = new EditColorCancelEventArgs(GetColor(index), index);
            OnEditingColor(e);

            if (!e.Cancel)
            {
                EditColor(index);
            }
        }

        #endregion Methods

        #region IColorEditor Interface

        [Category("Property Changed")]
        public event EventHandler ColorChanged
        {
            add => Events.AddHandler(_eventColorChanged, value);
            remove => Events.RemoveHandler(_eventColorChanged, value);
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public virtual Color Color
        {
            get => _color;
            set
            {
                int newIndex;

                _color = value;

                if (!value.IsEmpty)
                {
                    // the new color matches the color at the current index, so don't change the index
                    // this stops the selection hopping about if you have duplicate colors in a palette
                    // otherwise, if the colors don't match, then find the index that does
                    newIndex = GetColor(ColorIndex) == value ? ColorIndex : GetColorIndex(value);

                    if (newIndex == InvalidIndex)
                    {
                        newIndex = AddCustomColor(value);
                    }
                }
                else
                {
                    newIndex = InvalidIndex;
                }

                ColorIndex = newIndex;

                OnColorChanged(EventArgs.Empty);
            }
        }

        #endregion IColorEditor Interface
    }
}