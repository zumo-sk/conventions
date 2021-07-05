namespace Conventions.Interaction.Requests
{
    using System;

    public class UpsertTalkRequest
    {
        public string Title { get; set; }

        public string SpeakerId { get; set; } // userId of the speaker

        public string ConventionId { get; set; } // userId of the speaker

        public long? StartTime { get; set; }

        public long? EndTime { get; set; }

        public uint? Capacity { get; set; }
    }
}
