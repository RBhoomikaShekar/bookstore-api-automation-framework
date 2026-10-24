namespace BookStore.Api.Domain
{
    public class AppUser
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }   // "User" or "Admin"
    }
}
