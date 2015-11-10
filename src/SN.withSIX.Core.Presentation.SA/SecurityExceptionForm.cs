// <copyright company="SIX Networks GmbH" file="SecurityExceptionForm.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartAssembly.SmartExceptionsCore;
using SmartAssembly.SmartExceptionsCore.UI;

namespace SN.withSIX.Core.Presentation.SA
{
    public class SecurityExceptionForm : Form
    {
        readonly SecurityExceptionEventArgs securityExceptionEventArgs;
        Button continueButton;
        AutoHeightLabel errorMessage;
        HeaderControl headerControl1;
        PoweredBy poweredBy;
        Button quitButton;

        public SecurityExceptionForm() {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            //

            Icon = Win32.GetApplicationIcon();
            Text = GetConvertedString(Text);
            if (Text.Length == 0)
                Text = "Security Exception";

            foreach (Control control in Controls) {
                control.Text = GetConvertedString(control.Text);
                foreach (Control subControl in control.Controls)
                    subControl.Text = GetConvertedString(subControl.Text);
            }
        }

        public SecurityExceptionForm(SecurityExceptionEventArgs securityExceptionEventArgs) : this() {
            if (securityExceptionEventArgs == null)
                return;

            if (!securityExceptionEventArgs.CanContinue)
                continueButton.Visible = false;

            this.securityExceptionEventArgs = securityExceptionEventArgs;

            if (securityExceptionEventArgs.SecurityMessage.Length > 0)
                errorMessage.Text = securityExceptionEventArgs.SecurityMessage;
            else {
                var sb = new StringBuilder();
                sb.Append(
                    "%AppName% attempted to perform an operation not allowed by the security policy. To grant this application the required permission, contact your system administrator, or use the Microsoft .NET Framework Configuration tool.\n\n");

                if (securityExceptionEventArgs.CanContinue) {
                    sb.Append(
                        "If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately.\n\n");
                }

                sb.Append(securityExceptionEventArgs.SecurityException.Message);
                errorMessage.Text = GetConvertedString(sb.ToString());
            }

            var newClientHeigth = errorMessage.Bottom + 60;
            if (newClientHeigth > ClientSize.Height)
                ClientSize = new Size(ClientSize.Width, newClientHeigth);
        }

        string GetConvertedString(string s) {
            s = s.Replace("%AppName%", UnhandledExceptionHandler.ApplicationName);
            s = s.Replace("%CompanyName%", UnhandledExceptionHandler.CompanyName);
            return s;
        }

        void continueButton_Click(object sender, EventArgs e) {
            if (securityExceptionEventArgs != null)
                securityExceptionEventArgs.TryToContinue = true;
            Close();
        }

        void quitButton_Click(object sender, EventArgs e) {
            if (securityExceptionEventArgs != null)
                securityExceptionEventArgs.TryToContinue = false;
            Close();
        }

        #region Windows Form Designer generated code

        void InitializeComponent() {
            this.quitButton = new System.Windows.Forms.Button();
            this.continueButton = new System.Windows.Forms.Button();
            this.headerControl1 = new SmartAssembly.SmartExceptionsCore.UI.HeaderControl();
            this.errorMessage = new SmartAssembly.SmartExceptionsCore.UI.AutoHeightLabel();
            this.poweredBy = new SmartAssembly.SmartExceptionsCore.UI.PoweredBy();
            this.SuspendLayout();
            // 
            // quitButton
            // 
            this.quitButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.quitButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.quitButton.Location = new System.Drawing.Point(308, 188);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(100, 24);
            this.quitButton.TabIndex = 0;
            this.quitButton.Text = "&Quit";
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // continueButton
            // 
            this.continueButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.continueButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.continueButton.Location = new System.Drawing.Point(202, 188);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(100, 24);
            this.continueButton.TabIndex = 1;
            this.continueButton.Text = "&Continue";
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // headerControl1
            // 
            this.headerControl1.BackColor = System.Drawing.Color.FromArgb(((System.Byte) (36)), ((System.Byte) (96)),
                ((System.Byte) (179)));
            this.headerControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerControl1.ForeColor = System.Drawing.Color.White;
            this.headerControl1.IconState = SmartAssembly.SmartExceptionsCore.UI.IconState.Warning;
            this.headerControl1.Image = null;
            this.headerControl1.Location = new System.Drawing.Point(0, 0);
            this.headerControl1.Name = "headerControl1";
            this.headerControl1.Size = new System.Drawing.Size(418, 58);
            this.headerControl1.TabIndex = 7;
            this.headerControl1.TabStop = false;
            this.headerControl1.Text = "%AppName% attempted to perform an operation not allowed by the security policy.";
            // 
            // errorMessage
            // 
            this.errorMessage.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            this.errorMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.errorMessage.Location = new System.Drawing.Point(20, 72);
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Size = new System.Drawing.Size(382, 13);
            this.errorMessage.TabIndex = 14;
            this.errorMessage.Text = "errorMessage";
            this.errorMessage.UseMnemonic = false;
            // 
            // poweredBy
            // 
            this.poweredBy.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.poweredBy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.poweredBy.Location = new System.Drawing.Point(6, 186);
            this.poweredBy.Name = "poweredBy";
            this.poweredBy.Size = new System.Drawing.Size(120, 32);
            this.poweredBy.TabIndex = 15;
            this.poweredBy.TabStop = false;
            this.poweredBy.Text = "poweredBy1";
            // 
            // SecurityExceptionForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(418, 224);
            this.ControlBox = false;
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.headerControl1);
            this.Controls.Add(this.errorMessage);
            this.Controls.Add(this.poweredBy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SecurityExceptionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "%AppName%";
            this.ResumeLayout(false);
        }

        #endregion
    }
}