using System;

namespace SuperSee;

public class CommitteeAvailability
{
    public Guid CommitteeAvailabilityId { get; set; }
    public Guid SupervisorId { get; set; }
    public Supervisor Supervisor { get; set; }
    public DateTime AvailableDate { get; set; }
    public string TimeSlot { get; set; } // e.g., "10:00 AM - 12:00 PM" or just a specific time
}
