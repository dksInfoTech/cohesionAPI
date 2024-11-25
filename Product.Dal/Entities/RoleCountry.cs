﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Product.Dal.Interfaces;

namespace Product.Dal.Entities;
public class RoleCountry : IAuditableEntity
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


    [ForeignKey("UserRole")]

    public int UserRoleId { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    public string? UserLocation { get; set; }


    [StringLength(50, ErrorMessage = "Max length 50")]
    public string Location { get; set; }


    public bool Active { get; set; }

    public virtual UserRole UserRole { get; set; }
}