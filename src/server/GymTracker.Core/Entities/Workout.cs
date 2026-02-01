using System;
using System.Collections.Generic;

namespace GymTracker.Core.Entities
{
    public class Workout
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
