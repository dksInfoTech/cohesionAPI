using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities
{
    public class SourceFinancial
    {
        [Required]
        [ForeignKey(nameof(SourceEntity))]
        [StringLength(20, ErrorMessage = "Max length 20")]
        public required string Ticker { get; set; }

        public int EqyFundYear { get; set; }

        [StringLength(10, ErrorMessage = "Max length 10")]
        public string? Currency { get; set; }

        public decimal? Scaling { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        [ForeignKey(nameof(SourceFinancialCode))]
        public required string FinCode { get; set; }

        public decimal? FinValue { get; set; }

        public virtual SourceEntity SourceEntity { get; set; }
        public virtual SourceFinancialCode SourceFinancialCode { get; set; }
    }
}
