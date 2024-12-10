using System.Drawing;

namespace AltUI.Config
{
    public class LightTheme : ITheme
    {
        public Sizes Sizes { get; } = new Sizes();

        public Colors Colors { get; } = new Colors();

        public LightTheme()
        {
            Colors.GreyBackground = ThemeProvider.BackgroundColour; //Form Background
            Colors.HeaderBackground = Color.FromArgb(177, 180, 182);
            Colors.BlueBackground = Color.FromArgb(186, 197, 215);
            Colors.DarkBlueBackground = Color.FromArgb(172, 177, 186);
            Colors.DarkBackground = Color.FromArgb(160, 160, 160);
            Colors.MediumBackground = Color.FromArgb(169, 171, 173);
            Colors.LightBackground = Color.FromArgb(238, 238, 242); // Control Colour
            Colors.LighterBackground = Color.FromArgb(226, 226, 226); // Control Hover
            Colors.LightestBackground = Color.FromArgb(0, 0, 0);
            Colors.LightBorder = Color.FromArgb(201, 201, 201);
            Colors.DarkBorder = Color.FromArgb(220, 220, 220);
            Colors.LightText = Color.FromArgb(30, 30, 30); // Normal Text
            Colors.DisabledText = Color.FromArgb(113, 113, 113); // Disabled Text
            Colors.BlueHighlight = ThemeProvider.GetAccentColor(50); // Selected Control Borders
            Colors.BlueSelection = ThemeProvider.GetAccentColor(0); // DropDown Selection
            Colors.GreyHighlight = Color.FromArgb(113, 113, 113); // ComboBox Arrow
            Colors.GreySelection = Color.FromArgb(204, 206, 219); // Control Border
            Colors.DarkGreySelection = Color.FromArgb(202, 202, 202);
            Colors.DarkBlueBorder = Color.FromArgb(171, 181, 198);
            Colors.LightBlueBorder = Color.FromArgb(206, 217, 114);
            Colors.ActiveControl = Color.FromArgb(159, 178, 196);
            Colors.TransparentBackground = Color.FromArgb(231, 231, 232);
            Colors.DefaultBackground = Color.FromArgb(255, 255, 255);

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
