using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Product.Dal.Entities;

namespace Product.Web.Models.Proposal
{
    public class ProposalTeamMemberViewModel
    {
        public int Id { get; set; }

        public int ProposalId { get; set; }
        
        public string UserId { get; set; }
        
        public string Title { get; set; }

        public string Role { get; set; }

         public string? Decision { get; set; }

        public bool IsFinal { get; set; }

        public string? Comments { get; set; }

        public DateTime? LastDecisionDate { get; set; }

        public DateTime? ExpectedDecisionDate { get; set; }
       
        public string ProposalStatus { get; set; }

        public int ClientId { get; set; }

        public Image? UserImage { get; set; }

        public string AssignedBy { get; set; }
        public string UserRole { get; set; }
        public string UserName { get; set; }

        public string ClientName { get; set; }

        public Image? ClientImage { get; set; }

    }
}
