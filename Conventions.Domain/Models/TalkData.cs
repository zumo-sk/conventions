namespace Conventions.Domain.Models
{
    public class TalkData
    {
        public string Title { get; set; }

        public string SpeakerId { get; set; } // userId of the speaker

        public string ConventionId { get; set; }

        public long? StartTime { get; set; }

        public long? EndTime { get; set; }

        public uint? Capacity { get; set; }
    }
}
