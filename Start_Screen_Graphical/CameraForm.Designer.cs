namespace cameraTest_1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.AMC = new AxAXISMEDIACONTROLLib.AxAxisMediaControl();
            ((System.ComponentModel.ISupportInitialize)(this.AMC)).BeginInit();
            this.SuspendLayout();
            // 
            // AMC
            // 
            this.AMC.Enabled = true;
            this.AMC.Location = new System.Drawing.Point(-1, 2);
            this.AMC.Name = "AMC";
            this.AMC.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("AMC.OcxState")));
            this.AMC.Size = new System.Drawing.Size(984, 692);
            this.AMC.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 692);
            this.Controls.Add(this.AMC);
            this.Name = "Form1";
            this.Text = "CameraForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.AMC)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxAXISMEDIACONTROLLib.AxAxisMediaControl AMC;
    }
}