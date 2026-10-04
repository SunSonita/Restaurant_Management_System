using System;

namespace Resturant_Management.Data
{
    public static class UserSession
    {
        public static int UserID { get; set; } = 1;
        public static string Username { get; set; } = "admin";
        public static string FullName { get; set; } = "Administrator";
        public static string Role { get; set; } = "Admin";
        public static bool IsLoggedIn { get; set; } = true;

        public static bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public static bool IsManager => IsAdmin || string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);
        public static bool IsCashier => string.Equals(Role, "Cashier", StringComparison.OrdinalIgnoreCase);

        public static bool CanManageInventory => IsAdmin || IsManager;
        public static bool CanManageTables => IsAdmin || IsManager;
        public static bool CanManageUsers => IsAdmin;
        public static bool CanViewReports => true; // All authenticated staff can view reports

        public static void SetUser(int userId, string username, string fullName, string role)
        {
            UserID = userId;
            Username = username;
            FullName = fullName;
            Role = role;
            IsLoggedIn = true;
        }

        public static void Logout()
        {
            UserID = 0;
            Username = "";
            FullName = "";
            Role = "";
            IsLoggedIn = false;
        }
    }
}
