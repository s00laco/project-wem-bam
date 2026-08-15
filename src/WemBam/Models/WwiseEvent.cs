namespace WemBam.Models
{
    public class WwiseEvent
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ObjectPath { get; set; } = string.Empty;

        public string DurationType { get; set; } = string.Empty;

        public double? DurationMin { get; set; }

        public double? DurationMax { get; set; }
    }
}