namespace HealthCareAB_v1.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; }

        public bool IsLockedOut { get; set; }
    }
}
