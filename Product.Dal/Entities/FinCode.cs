using Product.Dal.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Product.Dal.Entities
{
    public class FinCode
    {
        [Key]
        [StringLength(100, ErrorMessage = "Max length 100")]
        public required string Code { get; set; }

        [StringLength(500, ErrorMessage = "Max length 500")]
        public required string FinTitle { get; set; }

        [ForeignKey(nameof(FinCodeGroup))]
        public string? GroupCode { get; set; }

        public FinStatementType FinStatementType { get; set; }

        public bool IsCalc {  get; set; }

        [Column(TypeName = "text")]
        public string? Formula { get; set; }

        public int SortOrder { get; set; }

        public string? Style { get; set; }

        public virtual FinCodeGroup? FinCodeGroup { get; set; }
    }
}
