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
public class Image : IAuditableEntity
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
    [StringLength(100, ErrorMessage = "Max length 100")]
    public string FileName { get; set; }

    [StringLength(10, ErrorMessage = "Max length 10")]
    public string Extension { get; set; }


    [StringLength(100, ErrorMessage = "Max length 100")]
    public string ContentType { get; set; }


    [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
    public int ContentSize { get; set; }

    /// <summary>
    /// like Client Image or Entity Image or UserImage
    /// </summary>
    public string ImageCategory { get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "Required")]
    public byte[] Content { get; set; }

    public int? ProposalId { get; set; }
}
