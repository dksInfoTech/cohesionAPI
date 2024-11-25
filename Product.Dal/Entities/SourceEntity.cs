using System.ComponentModel.DataAnnotations;

namespace Product.Dal.Entities
{
    public class SourceEntity
    {
        [Key]
        [StringLength(20, ErrorMessage = "Max length 20")]
        public required string SourceId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Max length 20")]
        public required string Ticker { get; set; }

        [StringLength(50, ErrorMessage = "Max length 50")]
        public required string TickerAndExchCode { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public required string LongCompanyName { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        public string? L1Industry { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        public string? L2Industry { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        public string? L3Industry { get; set; }

        [StringLength(100, ErrorMessage = "Max length 100")]
        public string? L4Industry { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? CompanyType { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? CountryOfDomicile { get; set; }

        public int? NoOfEmployees { get; set; }

        public decimal? CurrentMktCap { get; set; }

        public decimal? PeRatio { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? ParentTicker { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? ParentLongName { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? UltParentTicker { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? UltParentName { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? UltParentLongName { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? GlobalCompanyName { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? LeiName { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? CompanyCorpTicker { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? CompanyLegalName { get; set; }

        [StringLength(50, ErrorMessage = "Max length 255")]
        public string? CompParentRelation { get; set; }

        [StringLength(255, ErrorMessage = "Max length 255")]
        public string? LeiUltimateParent { get; set; }

        [StringLength(20, ErrorMessage = "Max length 20")]
        public string? LatestFiling { get; set; }
    }
}
