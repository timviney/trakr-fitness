using System;

namespace GymTracker.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PasswordHashed { get; set; } = null!;
    }
}
