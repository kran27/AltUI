/*

Copyright © 2018 - 2022 haltroy
Modified by kran27 for usage in AltUI (https://github.com/kran27/AltUI)
Use of this source code is governed by an MIT License that can be found in https://github.com/Haltroy/HTAlt/blob/master/LICENSE

*/

using AltUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AltUI.Controls
{
    public class DarkTabControl : TabControl
    {
        /// <summary>
        /// This control's original source.
        /// </summary>
        [Bindable(false)]
        [Category("Info")]
        [Description("This control's original source.")]
        public Uri SrcRepo { get; } = new Uri("https://github.com/haltroy/HTAlt");

        /// <summary>
        /// This option disables closing tabs.
        /// </summary>
        [Category("Behavior"), Browsable(true), Description("This option disables closing tabs.")]
        public bool DisableClose { get; set; }

        /// <summary>
        /// This option disables dragging tabs.
        /// </summary>
        [Category("Behavior"), Browsable(true), Description("This option disables dragging tabs.")]
        public bool DisableDragging { get; set; }

        private readonly StringFormat CenterSF;
        private TabPage predraggedTab;
        private int hoveringTabIndex;

        private SubClass scUpDown;
        private bool bUpDown;
        private bool hasFocus;

        public DarkTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            CenterSF = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            Padding = new Point(14, 4);
            AllowDrop = true;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            Alignment = TabAlignment.Top;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
            var hoveringTabs = Enumerable.Range(0, TabCount).Where(i => GetTabRect(i).IntersectsWith(mouseRect));

            if (hoveringTabs.Any())
            {
                var tabIndex = hoveringTabs.First();
                var tabBase = new Rectangle(new Point(GetTabRect(tabIndex).Location.X + 2, GetTabRect(tabIndex).Location.Y), new Size(GetTabRect(tabIndex).Width, GetTabRect(tabIndex).Height));
                var tabExitRectangle = new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15);

                if (tabExitRectangle.Contains(PointToClient(Cursor.Position)))
                {
                    if (!DisableClose)
                    {
                        TabPages.Remove(TabPages[tabIndex]);
                        return;
                    }
                }
            }

            predraggedTab = getPointedTab();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            predraggedTab = null;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (SelectedIndex == -1)
            {
                base.OnMouseMove(e);
                return;
            }

            // check whether they are hovering over a tab button
            var tabIndex = SelectedIndex;
            var tabBase = new Rectangle(new Point(GetTabRect(tabIndex).Location.X + 2, GetTabRect(tabIndex).Location.Y), new Size(GetTabRect(tabIndex).Width, GetTabRect(tabIndex).Height));

            var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
            var hoveringTabs = Enumerable.Range(0, TabCount).Where(i => GetTabRect(i).IntersectsWith(mouseRect));

            if (hoveringTabs.Any())
            {
                hoveringTabIndex = hoveringTabs.First();
            }

            if (e.Button == MouseButtons.Left && predraggedTab != null && !DisableDragging)
            {
                DoDragDrop(predraggedTab, DragDropEffects.Move);
            }

            if (e.Y < 25) // purely for performance reasons, only necessary for hovering button states
            {
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (hoveringTabIndex != -1)
            {
                hoveringTabIndex = -1;
                Invalidate();
            }

            base.OnLeave(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (hoveringTabIndex != -1)
            {
                hoveringTabIndex = -1;
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(ThemeProvider.Theme.Colors.GreyBackground);

            switch (hasFocus)
            {
                // ugly way to check whether the parent form has focus or not
                case false when FindForm() == Form.ActiveForm:
                    Invalidate(new Rectangle(0, 21, Width, 24));
                    hasFocus = true;
                    break;

                case true when FindForm() != Form.ActiveForm:
                    Invalidate(new Rectangle(0, 21, Width, 24));
                    hasFocus = false;
                    break;
            }

            for (var i = 0; i < TabCount; i++)
            {
                var tabBase = new Rectangle(new Point(GetTabRect(i).Location.X + 2, GetTabRect(i).Location.Y), new Size(GetTabRect(i).Width, GetTabRect(i).Height + 1));
                var tabSize = new Rectangle(tabBase.Location, new Size(tabBase.Width - 1, tabBase.Height - 4));

                // draw tab highlights
                g.FillRoundedRectangle(
                    hoveringTabIndex == i && SelectedIndex != i
                        ? new SolidBrush(ThemeProvider.Theme.Colors.LighterBackground)
                        : new SolidBrush(ThemeProvider.Theme.Colors.LightBackground), tabSize, 4, true);

                // if current selected tab
                if (SelectedIndex == i)
                {
                    var oldClip = g.Clip;
                    var clipRect = tabBase with { Height = tabBase.Height - 4 };
                    g.Clip = new Region(clipRect);

                    g.DrawRoundedRectangle(new Pen(ThemeProvider.Theme.Colors.GreySelection), tabSize, 4, true);

                    g.Clip = oldClip;

                    if (!DisableClose)
                    {
                        // hovering over selected tab button
                        if (new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15).Contains(PointToClient(Cursor.Position)))
                        {
                            g.FillRectangle(new SolidBrush(FindForm() == Form.ActiveForm ? ThemeProvider.Theme.Colors.BlueHighlight : ThemeProvider.Theme.Colors.DarkBackground),
                                new RectangleF((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15));
                        }

                        g.TextContrast = 0;

                        g.DrawString("×", new Font(Font.FontFamily, 15f), new SolidBrush(ThemeProvider.Theme.Colors.LightText),
                            tabBase with { X = (tabBase.Location.X + tabBase.Width) - (15 + 5), Y = tabBase.Location.Y - 3 }, CenterSF);
                    }
                }
                else
                {
                    // if hovering over a tab
                    if (hoveringTabIndex == i)
                    {
                        if (!DisableClose)
                        {
                            // hovering over hovered tab button
                            if (new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15).Contains(PointToClient(Cursor.Position)))
                            {
                                g.FillRectangle(new SolidBrush(ThemeProvider.Theme.Colors.BlueHighlight),
                                    new RectangleF((tabBase.Location.X + tabBase.Width) - (15 + 3), tabBase.Location.Y + 3, 15, 15));
                            }

                            g.TextContrast = 0;
                            g.DrawString("×", new Font(Font.FontFamily, 15f), new SolidBrush(ThemeProvider.Theme.Colors.LightText),
                                new Rectangle((tabBase.Location.X + tabBase.Width) - (15 + 5), tabBase.Location.Y - 3, tabBase.Width, tabBase.Height), CenterSF);
                        }
                    }
                }

                g.TextContrast = 12;
                g.DrawString(TabPages[i].Text, new Font(Font.FontFamily, Font.Size), new SolidBrush(ThemeProvider.Theme.Colors.LightText),
                    new Rectangle(tabBase.Location.X + 3, tabBase.Location.Y - 1, tabBase.Width, tabBase.Height + 1), CenterSF);
            }

            if (SelectedIndex >= 0 && e.ClipRectangle.Height > 24)
            {
                var selTabBase = GetTabRect(SelectedIndex);
                var selTabSize = selTabBase with
                {
                    X = selTabBase.X + 2, Width = selTabBase.Width - 1, Height = selTabBase.Height - 4
                };
                var fullRect = e.ClipRectangle with
                {
                    Y = e.ClipRectangle.Y + selTabBase.Height - 2, Width = e.ClipRectangle.Width - 1,
                    Height = e.ClipRectangle.Height - selTabBase.Height + 1
                };

                g.FillRoundedRectangle(new SolidBrush(ThemeProvider.Theme.Colors.LightBackground), fullRect, 4);
                g.DrawGapRoundedRectangle(new Pen(ThemeProvider.Theme.Colors.GreySelection), fullRect, 4,
                    selTabSize.Left, selTabSize.Right);
            }
            else
            {
                var fullRect = e.ClipRectangle with
                {
                    Width = e.ClipRectangle.Width - 1
                };

                g.FillRoundedRectangle(new SolidBrush(ThemeProvider.Theme.Colors.LightBackground), fullRect, 4, true);
                g.DrawRoundedRectangle(new Pen(ThemeProvider.Theme.Colors.GreySelection), fullRect, 4, true);
            }

            if (SelectedIndex != -1)
            {
                SelectedTab.BorderStyle = BorderStyle.None;
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            var draggedTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
            var pointedTab = getPointedTab();
            if (!DisableDragging)
            {
                if (draggedTab == predraggedTab && pointedTab != null)
                {
                    drgevent.Effect = DragDropEffects.Move;

                    if (pointedTab != draggedTab)
                    {
                        swapTabPages(draggedTab, pointedTab);
                    }
                }
            }
            base.OnDragOver(drgevent);
        }

        private TabPage getPointedTab()
        {
            checked
            {
                for (var i = 0; i <= TabPages.Count - 1; i++)
                {
                    if (GetTabRect(i).Contains(PointToClient(Cursor.Position)))
                    {
                        return TabPages[i];
                    }
                }
                return null;
            }
        }

        private void swapTabPages(TabPage src, TabPage dst)
        {
            if (!DisableDragging)
            {
                var srci = TabPages.IndexOf(src);
                var dsti = TabPages.IndexOf(dst);
                TabPages[dsti] = src;
                TabPages[srci] = dst;

                SelectedIndex = (SelectedIndex == srci) ? dsti : srci;
            }
            Refresh();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            FindUpDown();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            FindUpDown();
            UpdateUpDown();

            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            FindUpDown();
            UpdateUpDown();

            base.OnControlRemoved(e);
        }

        private void FindUpDown()
        {
            var bFound = false;
            var pWnd = Win32.GetWindow(Handle, Win32.GW_CHILD);

            while (pWnd != IntPtr.Zero)
            {
                var className = new char[33];
                var length = Win32.GetClassName(pWnd, className, 32);
                var s = new string(className, 0, length);

                if (s == "msctls_updown32")
                {
                    bFound = true;

                    if (!bUpDown)
                    {
                        scUpDown = new SubClass(pWnd, true);
                        scUpDown.SubClassedWndProc += new SubClass.SubClassWndProcEventHandler(scUpDown_SubClassedWndProc);

                        bUpDown = true;
                    }
                    break;
                }

                pWnd = Win32.GetWindow(pWnd, Win32.GW_HWNDNEXT);
            }

            if ((!bFound) && (bUpDown))
            {
                bUpDown = false;
            }
        }

        private void UpdateUpDown()
        {
            if (!bUpDown) return;
            if (!Win32.IsWindowVisible(scUpDown.Handle)) return;
            var rect = new Rectangle();

            Win32.GetClientRect(scUpDown.Handle, ref rect);
            Win32.InvalidateRect(scUpDown.Handle, ref rect, true);
        }

        private int scUpDown_SubClassedWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_PAINT:
                    {
                        var hDC = Win32.GetWindowDC(scUpDown.Handle);
                        var g = Graphics.FromHdc(hDC);

                        DrawIcons(g);

                        g.Dispose();
                        Win32.ReleaseDC(scUpDown.Handle, hDC);
                        m.Result = IntPtr.Zero;

                        var rect = new Rectangle();

                        Win32.GetClientRect(scUpDown.Handle, ref rect);
                        Win32.ValidateRect(scUpDown.Handle, ref rect);
                    }
                    return 1;
            }

            return 0;
        }

        internal void DrawIcons(Graphics g)
        {
            var TabControlArea = ClientRectangle;
            var r0 = new Rectangle();
            Win32.GetClientRect(scUpDown.Handle, ref r0);

            Brush br = new SolidBrush(ThemeProvider.Theme.Colors.LighterBackground);
            g.FillRectangle(br, r0);
            br.Dispose();

            g.DrawString("◀", new Font(Font.FontFamily, 12f),
                new SolidBrush(ThemeProvider.Theme.Colors.DisabledText), r0);

            g.DrawString("▶", new Font(Font.FontFamily, 12f),
                new SolidBrush(ThemeProvider.Theme.Colors.DisabledText),
                new Rectangle(r0.X + 20, r0.Y, r0.Width, r0.Height));
        }
    }

    internal static class Win32
    {
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;
        public const int GW_CHILD = 5;

        public const int WM_NCCALCSIZE = 0x83;
        public const int WM_WINDOWPOSCHANGING = 0x46;
        public const int WM_PAINT = 0xF;
        public const int WM_CREATE = 0x1;
        public const int WM_NCCREATE = 0x81;
        public const int WM_NCPAINT = 0x85;
        public const int WM_PRINT = 0x317;
        public const int WM_DESTROY = 0x2;
        public const int WM_SHOWWINDOW = 0x18;
        public const int WM_SHARED_MENU = 0x1E2;
        public const int HC_ACTION = 0;
        public const int WH_CALLWNDPROC = 4;
        public const int GWL_WNDPROC = -4;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowDC(IntPtr handle);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ReleaseDC(IntPtr handle, IntPtr hDC);

        [DllImport("Gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hwnd, char[] className, int maxCount);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetClientRect(IntPtr hwnd, ref RECT lpRect);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetClientRect(IntPtr hwnd, [In, Out] ref Rectangle rect);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool UpdateWindow(IntPtr hwnd);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool InvalidateRect(IntPtr hwnd, ref Rectangle rect, bool bErase);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool ValidateRect(IntPtr hwnd, ref Rectangle rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref Rectangle rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS
        {
            public RECT rgc;
            public WINDOWPOS wndpos;
        }
    }

    internal class SubClass : NativeWindow
    {
        public delegate int SubClassWndProcEventHandler(ref Message m);

        public event SubClassWndProcEventHandler SubClassedWndProc;

        public SubClass(IntPtr Handle, bool _SubClass)
        {
            AssignHandle(Handle);
            SubClassed = _SubClass;
        }

        public bool SubClassed { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (SubClassed)
            {
                if (OnSubClassedWndProc(ref m) != 0)
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private int OnSubClassedWndProc(ref Message m)
        {
            return SubClassedWndProc?.Invoke(ref m) ?? 0;
        }
    }
}