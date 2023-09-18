using System;

namespace sampleapi
{
    internal class Parameters
    {
        public string? BaseRoot { get; set; }
        public int Port { get; set; }
        public string? ConnectionString { get; set; }

        public Parameters()
        {
            BaseRoot = Environment.GetEnvironmentVariable("BASE_ROOT");
            ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentOutOfRangeException("CONNECTION_STRING", "Cannot be empty");
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!int.TryParse(port, out var p))
                throw new ArgumentOutOfRangeException("PORT", "Value is empty or invalid");
            Port = p;
        }
    }
}
