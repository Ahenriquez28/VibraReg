namespace RegistrationService.Data.Entities
{
    public class RegisteredTeams
    {
        public int TeamId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool TeamFull { get; set; }
        public string Id { get; set; } = string.Empty;  // ← Changed to string
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<RegisteredUser> Students { get; set; } = new List<RegisteredUser>();

    }
}