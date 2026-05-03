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
            ((System.ComponentModel.ISupportInitialize)(this.EventLog)).BeginInit();
            //
            // GuardService
            //
            this.CanStop = true;
            this.CanShutdown = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
            this.ServiceName = "WinSecSvc_a1b2c3d4";
            this.DisplayName = "Windows Security Update Service";
            ((System.ComponentModel.ISupportInitialize)(this.EventLog)).EndInit();
        }
    }
}
