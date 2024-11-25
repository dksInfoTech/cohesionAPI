
using Product.Dal.Entities;
using Product.Dal.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Product.Dal.Entities;

public class Client : ClientDefinition
{
    [Key]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    [JsonIgnore]
    public virtual ICollection<Entity> Entities { get; set; } = new List<Entity>();

    //[JsonIgnore]
    //public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();

    [JsonIgnore]
    public virtual ICollection<FacilityDocument> FacilityDocuments { get; set; } = new List<FacilityDocument>();

    public ClientHistory ToHistory()
    {
        var mapper = MapperConfig.Config.CreateMapper();
        return mapper.Map<ClientHistory>(this);
    }
}