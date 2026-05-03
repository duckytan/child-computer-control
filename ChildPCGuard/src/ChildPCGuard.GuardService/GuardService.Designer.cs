namespace ChildPCGuard.GuardService
{
    partial class GuardService
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.EventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.EventLog)).BeginInit();
            // 
            // EventLog
            // 
            this.EventLog.Log = "Application";
            this.EventLog.Source = "WinSecSvc_a1b2c3d4";
            // 
            // GuardService
            // 
            this.ServiceName = "WinSecSvc_a1b2c3d4";
            ((System.ComponentModel.ISupportInitialize)(this.EventLog)).EndInit();
        }
    }
}
