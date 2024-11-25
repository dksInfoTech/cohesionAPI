using Product.Dal.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Product.Dal.Entities;

public class EarlyAlert : IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [StringLength(50, ErrorMessage = "Max length 50")]
        public string CreatedBy { get; set; }

        [StringLength(50, ErrorMessage = "Max length 50")]
        public string? ModifiedBy { get; set; }

        public int Version { get; set; }
       
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Title { get; set; }
        public string Comments { get; set; }
        public string Metric { get; set; }
    }

