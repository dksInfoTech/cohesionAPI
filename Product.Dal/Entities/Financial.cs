using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities
{
    public class Financial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(FinancialStatement))]
        public required int FinancialStatementId { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        [ForeignKey(nameof(Entities.FinCode))]
        public required string FinCode { get; set; }

        public decimal? FinValue { get; set; }

        public virtual FinancialStatement FinancialStatement { get; set; }
        public virtual Entities.FinCode Code { get; set; }
    }
}
