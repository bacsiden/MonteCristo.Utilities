namespace MonteCristo.MongoDB.Framework.Filter
{
    public class BaseFilter
    {
        public string query { get; set; }
        public string status { get; set; }
        public int start { get; set; }
        public int limit { get; set; } = 10;
        public int asc { get; set; } = 1;
        public string orderby { get; set; }
    }
}
