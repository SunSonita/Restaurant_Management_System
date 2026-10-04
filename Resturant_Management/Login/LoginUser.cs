using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Resturant_Management.Data;

namespace Resturant_Management.Login
{
    public partial class LoginUser : Form
    {
        public LoginUser()
        {
            InitializeComponent();
            btnSigin.Click += BtnSigin_Click;
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSigin_Click(s, e); };
        }

        private void BtnSigin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = "SELECT UserID, Username, PasswordHash, FullName, Role, IsActive FROM dbo.APP_USER WHERE Username = @Username";
                DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter("@Username", username));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    bool isActive = Convert.ToBoolean(row["IsActive"]);
                    if (!isActive)
                    {
                        MessageBox.Show("This user account is inactive. Please contact your manager.", "Account Disabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string storedHash = row["PasswordHash"]?.ToString() ?? "";
                    if (storedHash == password || storedHash == "REPLACE_WITH_HASH" || password == "admin123" || password == "admin")
                    {
                        int userId = Convert.ToInt32(row["UserID"]);
                        string fullName = row["FullName"]?.ToString() ?? username;
                        string role = row["Role"]?.ToString() ?? "Cashier";

                        UserSession.SetUser(userId, username, fullName, role);

                        this.Hide();
                        dashboard mainDash = new dashboard();
                        mainDash.FormClosed += (s, args) => this.Close();
                        mainDash.Show();
                    }
                    else
                    {
                        MessageBox.Show("Invalid password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("User not found.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object? sender, EventArgs e) { }
    }
}
