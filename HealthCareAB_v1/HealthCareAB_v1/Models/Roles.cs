namespace HealthCareAB_v1.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        // Helper method to validate roles
        public static bool IsValidRole(string role)
        {
            return role == Admin || role == User;
        }

        public static IReadOnlyList<string> AllRoles => new[] { Admin, User };
    }
}

