namespace Product.Web.Models.News
{
    public class NewsResponse
    {
        public string Category { get; set; }
        public long DateTime { get; set; }  // UNIX timestamp
        public string Headline { get; set; }
        public int Id { get; set; }
        public string Image { get; set; }
        public string Related { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
    }
}
