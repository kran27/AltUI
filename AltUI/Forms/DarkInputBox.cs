using AltUI.Icons;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AltUI.Forms
{
    public partial class DarkInputBox : DarkDialog
    {
        #region Field Region

        private string _message;
        private int _maximumWidth = 350;

        #endregion

        #region Property Region

        [Description("Determines the maximum width of the message box when it autosizes around the displayed message.")]
        [DefaultValue(350)]
        public int MaximumWidth
        {
            get => _maximumWidth;
            set
            {
                _maximumWidth = value;
                CalculateSize();
            }
        }

        #endregion

        #region Constructor Region

        public DarkInputBox()
        {
            InitializeComponent();
        }

        public DarkInputBox(string message, string title, DarkMessageBoxIcon icon, DarkDialogButton buttons, Icon formIcon)
            : this()
        {
            Text = title;
            _message = message;

            DialogButtons = buttons;
            SetIcon(icon);
            if (formIcon != null)
            {
                ShowInTaskbar = true;
                Icon = formIcon;
            }
        }

        public DarkInputBox(string message, Icon formIcon)
            : this(message, null, DarkMessageBoxIcon.None, DarkDialogButton.Ok, formIcon)
        { }

        public DarkInputBox(string message, string title, Icon formIcon)
            : this(message, title, DarkMessageBoxIcon.None, DarkDialogButton.Ok,formIcon)
        { }

        public DarkInputBox(string message, string title, DarkDialogButton buttons, Icon formIcon)
            : this(message, title, DarkMessageBoxIcon.None, buttons,formIcon)
        { }

        public DarkInputBox(string message, string title, DarkMessageBoxIcon icon, Icon formIcon)
            : this(message, title, icon, DarkDialogButton.Ok,formIcon)
        { }

        #endregion

        #region Static Method Region

        public static DialogResult ShowInformation(string message, string caption, ref string output, DarkDialogButton buttons = DarkDialogButton.Ok, Icon formIcon = null)
        {
            return ShowDialog(message, caption, ref output, DarkMessageBoxIcon.Information, buttons, formIcon);
        }

        public static DialogResult ShowWarning(string message, string caption, ref string output, DarkDialogButton buttons = DarkDialogButton.Ok, Icon formIcon = null)
        {
            return ShowDialog(message, caption, ref output, DarkMessageBoxIcon.Warning, buttons, formIcon);
        }

        public static DialogResult ShowError(string message, string caption, ref string output, DarkDialogButton buttons = DarkDialogButton.Ok, Icon formIcon = null)
        {
            return ShowDialog(message, caption, ref output, DarkMessageBoxIcon.Error, buttons,formIcon);
        }

        private static DialogResult ShowDialog(string message, string caption, ref string output, DarkMessageBoxIcon icon, DarkDialogButton buttons, Icon formIcon)
        {
            using var dlg = new DarkInputBox(message, caption, icon, buttons,formIcon);
            dlg.txtInput.Focus();
            var result = dlg.ShowDialog();
            output = dlg.txtInput.Text;
            return result;
        }

        #endregion

        #region Method Region

        private void SetIcon(DarkMessageBoxIcon icon)
        {
            switch (icon)
            {
                case DarkMessageBoxIcon.None:
                    picIcon.Visible = false;
                    lblText.Left = 10;
                    break;
                case DarkMessageBoxIcon.Information:
                    picIcon.Image = MessageBoxIcons.info;
                    break;
                case DarkMessageBoxIcon.Warning:
                    picIcon.Image = MessageBoxIcons.warning;
                    break;
                case DarkMessageBoxIcon.Error:
                    picIcon.Image = MessageBoxIcons.error;
                    break;
            }
        }

        private void CalculateSize()
        {
            var width = 260; var height = 154;

            // Reset form back to original size
            Size = new Size(width, height);

            lblText.Text = string.Empty;
            lblText.AutoSize = true;
            lblText.Text = _message;

            // Set the minimum dialog size to whichever is bigger - the original size or the buttons.
            var minWidth = Math.Max(width, TotalButtonSize + 15);

            // Calculate the total size of the message
            var totalWidth = lblText.Right + 25;

            // Make sure we're not making the dialog bigger than the maximum size
            if (totalWidth < _maximumWidth)
            {
                // Width is smaller than the maximum width.
                // This means we can have a single-line message box.
                // Move the label to accomodate this.
                width = totalWidth;
                lblText.Top = picIcon.Top + (picIcon.Height / 2) - (lblText.Height / 2);
            }
            else
            {
                // Width is larger than the maximum width.
                // Change the label size and wrap it.
                width = _maximumWidth;
                var offsetHeight = Height - picIcon.Height;
                lblText.AutoUpdateHeight = true;
                lblText.Width = width - lblText.Left - 25;
                height = offsetHeight + lblText.Height;
            }

            // Force the width to the minimum width
            if (width < minWidth)
                width = minWidth;

            txtInput.Location = new Point(25, txtInput.Location.Y);
            txtInput.Width = width - 50;
            txtInput.Focus();

            // Set the new size of the dialog
            Size = new Size(width, height);
        }

        #endregion

        #region Event Handler Region

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CalculateSize();
        }

        #endregion
    }
}