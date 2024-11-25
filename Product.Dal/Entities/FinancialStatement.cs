using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Product.Dal.Enums;

namespace Product.Dal.Entities
{
    public class FinancialStatement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Entity))]
        public required int EntityId { get; set; }

        [Required]
        [ForeignKey(nameof(FinancialExtractJob))]
        public required int ExtractJobId { get; set; }

        public int FinancialYear { get; set; }

        public FinancialType FinancialType { get; set; }

        public virtual Entity Entity { get; set; }

        public virtual FinancialExtractJob FinancialExtractJob { get; set; }
    }
}
