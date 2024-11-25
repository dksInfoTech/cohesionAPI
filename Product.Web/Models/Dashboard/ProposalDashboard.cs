
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Product.Dal.Entities;

namespace Product.Web.Models.Dashboard;

public class ProposalDashboard
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime DateOfEvent { get; set; }
    public DateTime? LastContributedDate { get; set; }
    public string LastContributorName { get; set; }
    public string Status { get; set; }
    public string? Decision { get; set; }
    public bool IsClientUpdate { get; set; }
    public string ClientName { get; set; }
    public int ClientId { get; set; }
    public string ProposalInfo { get; set; }
    public ICollection<ProposalTeamMember> ProposalTeamMembers { get; set; }
    public IEnumerable<string> ProposalEventTypes { get; set; }
}