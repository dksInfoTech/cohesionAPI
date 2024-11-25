using Product.Dal.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dal.Entities
{
    public class FinancialExtractJob
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Entity))]
        public required int EntityId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Max length 50")]
        public required string JobId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Max length 100")]
        public required string FileName { get; set; }

        public FdeStage Stage { get; set; }

        public FdeStatus Status { get; set; }

        public bool IsCompleted { get; set; }

        [StringLength(50, ErrorMessage = "Max length 50")]
        public required string SubmittedBy { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public virtual Entity Entity { get; set; }
    }
}
