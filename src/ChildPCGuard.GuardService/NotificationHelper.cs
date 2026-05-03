using System;
using System.Windows.Forms;
using ChildPCGuard.Shared;

namespace ChildPCGuard.GuardService
{
    public class NotificationHelper
    {
        private readonly AppConfiguration _config;

        public NotificationHelper(AppConfiguration config)
        {
            _config = config;
        }

        public void ShowWarning(string message)
        {
            try
            {
                MessageBox.Show(message, "提醒",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
            catch { }
        }

        public void ShowError(string message)
        {
            try
            {
                MessageBox.Show(message, "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
            }
            catch { }
        }
    }
}
