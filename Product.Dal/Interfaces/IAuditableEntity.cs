namespace Product.Dal.Interfaces;

public interface IAuditableEntity
{
        DateTime CreatedDate { get; set; }

        DateTime? ModifiedDate { get; set; }

        string CreatedBy { get; set; }

        string? ModifiedBy { get; set; }

        int Version { get; set; }
}

public interface IFacilityAuditableEntity
{
        DateTime CreatedDate { get; set; }

        DateTime? ModifiedDate { get; set; }

        string CreatedBy { get; set; }

        string? ModifiedBy { get; set; }
}
