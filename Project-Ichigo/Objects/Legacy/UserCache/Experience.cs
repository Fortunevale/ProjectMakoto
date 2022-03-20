namespace Project_Ichigo.Objects.Legacy;

internal static class Experience
{
    internal class MultiplierInfo
    {
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public bool CanTimeout { get; set; }
        public DateTime Timeout { get; set; }
    }
}
