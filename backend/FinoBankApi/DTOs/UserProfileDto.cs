namespace FinoBankApi.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Pesel { get; set; } 
        public string City { get; set; } 
        public string Street { get; set; } 
        public string ZipCode { get; set; } 
        public DateTime CreatedAt { get; set; }
        public bool Is2faEnabled { get; set; } 
        public string Role { get; set; }
    }
}