using System;

namespace sampleapi
{
    public class SimpleMessage
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
