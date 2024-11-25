using System.ComponentModel.DataAnnotations;

namespace Product.Dal.Entities
{
    public class SourceFinancialCode
    {
        [Key]
        [StringLength(100, ErrorMessage = "Max length 100")]
        public required string FinCode { get; set; }

        [StringLength(500, ErrorMessage = "Max length 500")]
        public required string Title { get; set; }
    }
}
