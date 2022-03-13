﻿using System.Drawing;

namespace AltUI.Config
{
    public class DarkTheme : ITheme
    {
        public Sizes Sizes { get; } = new Sizes();

        public Colors Colors { get; } = new Colors();

        public DarkTheme()
        {
            Colors.GreyBackground = Color.FromArgb(16, 16, 17); //Form Background
            Colors.HeaderBackground = Color.FromArgb(57, 60, 62);
            Colors.BlueBackground = Color.FromArgb(66, 77, 95);
            Colors.DarkBlueBackground = Color.FromArgb(52, 57, 66);
            Colors.DarkBackground = Color.FromArgb(43, 43, 43);
            Colors.MediumBackground = Color.FromArgb(49, 51, 53);
            Colors.LightBackground = Color.FromArgb(26, 26, 29); // Control Colour
            Colors.LighterBackground = Color.FromArgb(33, 33, 37); // Control Hover
            Colors.LightestBackground = Color.FromArgb(230, 230, 230);
            Colors.LightBorder = Color.FromArgb(81, 81, 81);
            Colors.DarkBorder = Color.FromArgb(32, 32, 32);
            Colors.LightText = Color.FromArgb(213, 213, 213); // Normal Text
            Colors.DisabledText = Color.FromArgb(101, 101, 101); // Disabled Text
            Colors.BlueHighlight = Color.FromArgb(0, 122, 204); // Blue Borders
            Colors.BlueSelection = Color.FromArgb(30, 71, 112); // Blue Selection
            Colors.GreyHighlight = Color.FromArgb(146, 146, 146); // ComboBox Arrow
            Colors.GreySelection = Color.FromArgb(47, 47, 55); // Control Border
            Colors.DarkGreySelection = Color.FromArgb(82, 82, 82);
            Colors.DarkBlueBorder = Color.FromArgb(51, 61, 78);
            Colors.LightBlueBorder = Color.FromArgb(86, 97, 114);
            Colors.ActiveControl = Color.FromArgb(159, 178, 196);

            Sizes.Padding = 10;
            Sizes.ScrollBarSize = 15;
            Sizes.ArrowButtonSize = 15;
            Sizes.MinimumThumbSize = 11;
            Sizes.CheckBoxSize = 12;
            Sizes.RadioButtonSize = 12;
            Sizes.ToolWindowHeaderSize = 25;
            Sizes.DocumentTabAreaSize = 24;
            Sizes.ToolWindowTabAreaSize = 21;

        }
    }
}
