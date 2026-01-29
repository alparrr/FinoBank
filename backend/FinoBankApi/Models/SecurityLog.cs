public class SecurityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public string IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}