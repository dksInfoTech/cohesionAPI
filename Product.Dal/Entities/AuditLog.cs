using Microsoft.EntityFrameworkCore;
using Product.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Product.Dal.Entities;

public class AuditLog : IAuditLog
{
    [Key]
    public long Id { get; set; }

    public DateTime Timestamp { get; set; }

    [JsonIgnore]
    [ForeignKey("User")]
    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Username { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string HostAddress { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string HostName { get; set; }

    [StringLength(500, ErrorMessage = "Max length 500")]
    public string Action { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Section { get; set; }


    [ForeignKey("Client")]
    public int? ClientId { get; set; }


    [StringLength(500, ErrorMessage = "Max length 500")]
    public string Comment { get; set; }


    public bool IsAuditNote { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string Category { get; set; }

    public int? ProposalId { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }

    [NotMapped]
    public string UserFullName => User?.Name;
}