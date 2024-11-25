namespace Product.Web.Models.Financial
{
    public class FinancialItem
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public required string Type { get; set; }
        public required string FinCode { get; set; }
        public required string FinTitle { get; set; }
        public decimal? FinValue { get; set; }
        public bool? IsEdited {  get; set; }
    }
}
