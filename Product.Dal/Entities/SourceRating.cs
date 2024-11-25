using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Dal.Entities
{
    public class SourceRating
    {
        [Key]
        [ForeignKey(nameof(SourceEntity))]
        [StringLength(20, ErrorMessage = "Max length 20")]
        public required  string Ticker { get; set; }

        [StringLength(10, ErrorMessage = "Max length 10")]
        public string? BBRating { get; set; }

        public DateTime? BBRatingDate { get; set; }

        [StringLength(10, ErrorMessage = "Max length 10")]
        public string? BBRatingPrior { get; set; }

        [StringLength(10, ErrorMessage = "Max length 10")]
        public string? MoodysRating { get; set; }

        public DateTime? MoodysRatingDate { get; set; }

        [StringLength(10, ErrorMessage = "Max length 10")]
        public string? MoodysOutlook { get; set; }

        public DateTime? MoodysOutlookDate { get; set; }

        public virtual SourceEntity SourceEntity { get; set; }
    }
}
