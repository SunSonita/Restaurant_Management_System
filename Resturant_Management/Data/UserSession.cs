namespace Resturant_Management.Data
{
    public static class UserSession
    {
        public static int UserID { get; set; } = 1;
        public static string Username { get; set; } = "admin";
        public static string FullName { get; set; } = "Administrator";
        public static string Role { get; set; } = "Admin";
        public static bool IsLoggedIn { get; set; } = true;

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
