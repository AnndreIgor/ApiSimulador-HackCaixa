namespace ApiSimulador.Options
{
    public class EventHubOptions
    {
        public string? ConnectionString { get; set; }
        public string? EventHubName { get; set; }
        public List<string>? TargetRoutes { get; set; }
    }
}
