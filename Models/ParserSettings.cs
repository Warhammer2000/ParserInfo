namespace Parser.Models
{
    public class ParserSettings
    {
        public int DefaultPageSize { get; set; }
        public int MaxParallelism { get; set; }
        public int CacheDurationMinutes { get; set; }
    }
}
