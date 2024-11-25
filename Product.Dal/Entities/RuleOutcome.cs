using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class RuleOutcome : IAuditableEntity
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


    [Required(ErrorMessage = "Required")]
    [StringLength(50, ErrorMessage = "Max length 50")]
    //[StringRange(AllowedValues = new[] { RuleExceptionStatus.Approved, RuleExceptionStatus.Declined, RuleExceptionStatus.ForApproval })]
    public string Status { get; set; }


    [StringLength(500, ErrorMessage = "Max length 500")]
    public string Reason { get; set; }


    [Required(ErrorMessage = "Required")]
    public int SourceId { get; set; }


    public DateTime DueDate { get; set; }


    [Required(ErrorMessage = "Required")]
    [ForeignKey("User")]
    public string Approver { get; set; }

    [NotMapped]
    public string ApproverName => User?.Name;

    [JsonIgnore]
    public virtual User User { get; set; }
}

public static class RuleExceptionStatus
{
    public const string ForApproval = "For Approval";
    public const string Approved = "Approved";
    public const string Declined = "Declined";

    public static readonly List<string> Decisions = new List<string> { Approved, Declined };
}
