namespace Resturant_Management.Login
{
    partial class LoginUser
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
            Guna.UI2.WinForms.Guna2CirclePictureBox PicProfile;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginUser));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            panel1 = new Panel();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            label1 = new Label();
            btnSigin = new Guna.UI2.WinForms.Guna2Button();
            txtPassword = new Guna.UI2.WinForms.Guna2TextBox();
            txtUsername = new Guna.UI2.WinForms.Guna2TextBox();
            PicProfile = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            ((System.ComponentModel.ISupportInitialize)PicProfile).BeginInit();
            panel1.SuspendLayout();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // PicProfile
            // 
            PicProfile.BackColor = Color.Transparent;
            PicProfile.BorderStyle = BorderStyle.FixedSingle;
            PicProfile.Image = (Image)resources.GetObject("PicProfile.Image");
            PicProfile.ImageRotate = 0F;
            PicProfile.InitialImage = null;
            PicProfile.Location = new Point(135, 91);
            PicProfile.Name = "PicProfile";
            PicProfile.ShadowDecoration.CustomizableEdges = customizableEdges1;
            PicProfile.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            PicProfile.Size = new Size(151, 153);
            PicProfile.TabIndex = 3;
            PicProfile.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = Color.SkyBlue;
            panel1.Controls.Add(guna2Panel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 628);
            panel1.TabIndex = 0;
            // 
            // guna2Panel1
            // 
            guna2Panel1.BackColor = Color.White;
            guna2Panel1.BorderColor = Color.White;
            guna2Panel1.BorderRadius = 8;
            guna2Panel1.Controls.Add(label1);
            guna2Panel1.Controls.Add(btnSigin);
            guna2Panel1.Controls.Add(txtPassword);
            guna2Panel1.Controls.Add(txtUsername);
            guna2Panel1.Controls.Add(PicProfile);
            guna2Panel1.CustomBorderColor = Color.White;
            guna2Panel1.CustomizableEdges = customizableEdges8;
            guna2Panel1.FillColor = Color.White;
            guna2Panel1.Location = new Point(185, 38);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges9;
            guna2Panel1.Size = new Size(411, 545);
            guna2Panel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(121, 34);
            label1.Name = "label1";
            label1.Size = new Size(176, 31);
            label1.TabIndex = 7;
            label1.Text = "Authentication";
            label1.Click += label1_Click;
            // 
            // btnSigin
            // 
            btnSigin.BorderRadius = 5;
            btnSigin.CustomizableEdges = customizableEdges2;
            btnSigin.DisabledState.BorderColor = Color.DarkGray;
            btnSigin.DisabledState.CustomBorderColor = Color.DarkGray;
            btnSigin.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnSigin.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnSigin.FillColor = Color.Blue;
            btnSigin.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSigin.ForeColor = Color.White;
            btnSigin.Location = new Point(54, 417);
            btnSigin.Name = "btnSigin";
            btnSigin.ShadowDecoration.CustomizableEdges = customizableEdges3;
            btnSigin.Size = new Size(319, 42);
            btnSigin.TabIndex = 6;
            btnSigin.Text = "Sign In";
            // 
            // txtPassword
            // 
            txtPassword.BorderColor = Color.Gray;
            txtPassword.BorderRadius = 5;
            txtPassword.CustomizableEdges = customizableEdges4;
            txtPassword.DefaultText = "";
            txtPassword.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtPassword.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtPassword.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Font = new Font("Segoe UI", 9F);
            txtPassword.ForeColor = Color.Gray;
            txtPassword.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Location = new Point(54, 335);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderForeColor = Color.Gray;
            txtPassword.PlaceholderText = "Password";
            txtPassword.SelectedText = "";
            txtPassword.ShadowDecoration.CustomizableEdges = customizableEdges5;
            txtPassword.Size = new Size(319, 53);
            txtPassword.TabIndex = 5;
            // 
            // txtUsername
            // 
            txtUsername.BorderColor = Color.Gray;
            txtUsername.BorderRadius = 5;
            txtUsername.CustomizableEdges = customizableEdges6;
            txtUsername.DefaultText = "";
            txtUsername.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtUsername.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtUsername.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtUsername.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtUsername.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsername.Font = new Font("Segoe UI", 9F);
            txtUsername.ForeColor = Color.Gray;
            txtUsername.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsername.Location = new Point(54, 265);
            txtUsername.Margin = new Padding(3, 4, 3, 4);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderForeColor = Color.Gray;
            txtUsername.PlaceholderText = "Username";
            txtUsername.SelectedText = "";
            txtUsername.ShadowDecoration.CustomizableEdges = customizableEdges7;
            txtUsername.Size = new Size(319, 53);
            txtUsername.TabIndex = 4;
            // 
            // LoginUser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 628);
            Controls.Add(panel1);
            Name = "LoginUser";
            Text = "LoginUser";
            ((System.ComponentModel.ISupportInitialize)PicProfile).EndInit();
            panel1.ResumeLayout(false);
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2TextBox txtPassword;
        private Guna.UI2.WinForms.Guna2TextBox txtUsername;
        private Label label1;
        private Guna.UI2.WinForms.Guna2Button btnSigin;
    }
}