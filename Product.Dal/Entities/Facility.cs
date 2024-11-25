using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class Facility : IFacilityAuditableEntity
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

    public int FacilityId { get; set; }

    [Required(ErrorMessage = "Required")]
    [ForeignKey("Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Required")]
    [ForeignKey("Proposal")]
    public int ProposalId { get; set; }

    [ForeignKey("FacilityDocument")]
    public int? FacilityDocumentId { get; set; }

    [ForeignKey("InterchangableLimit")]
    public int? InterchangableLimitId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDraft { get; set; }

    public bool IsLatest { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsMarkForClosure { get; set; }

    public string? WorkflowId { get; set; }

    public string FacilityInfo { get; set; }
   
    public string? Comments { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }

    [JsonIgnore]
    public virtual Proposal Proposal { get; set; }
}

// Proposal Draft / Rework  == IsDraft true and IsLatest == false
// complete IsLatest == true and false
// once proposal is completed and 
// user started new proposal then pull last islatest=true record as per proposal id