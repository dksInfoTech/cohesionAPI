using Product.Dal.Enums;
using System.ComponentModel.DataAnnotations;


namespace Product.Dal.Entities
{
    public class FinCodeGroup
    {
        [Key]
        [StringLength(100, ErrorMessage = "Max length 100")]
        public required string GroupCode { get; set; }

        [StringLength(500, ErrorMessage = "Max length 500")]
        public required string GroupTitle { get; set; }

        public FinStatementType FinStatementType { get; set; }

        public int SortOrder { get; set; }

        public string? Style { get; set; }
    }
}
