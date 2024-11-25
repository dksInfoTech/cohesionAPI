using Product.Dal.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities
{
    public class FinancialExtractJobStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(FinancialExtractJob))]
        public required int ExtractJobId { get; set; }

        public FdeStage Stage { get; set; }

        public FdeStatus Status { get; set; }

        [Column(TypeName = "text")]
        public string? Details { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual FinancialExtractJob FinancialExtractJob { get; set; }
    }
}
