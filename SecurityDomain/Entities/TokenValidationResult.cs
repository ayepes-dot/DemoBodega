namespace SecurityDomain.Entities
{
    public class TokenValidationResult
    {
        public bool Valid { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
