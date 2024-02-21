using Arch_TL.DAL.Models;
using Arch_TL.DAL.Context.DbPartials;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Arch_TL.DAL.Attributes;

namespace Arch_TL.DAL.Context.Entities;

[Table("biology_domain")]
public class BiologyDomain : BaseEntity
{
    [Key]
    [Column(Db.BiologyDomain.Columns.Id)]
    public int Id { get; set; }

    [SearchText]
    [Column(Db.BiologyDomain.Columns.Name)]
    public string Name { get; set; }

    [SearchText]
    [Column(Db.BiologyDomain.Columns.Description)]
    public string Description { get; set; }

    [ActiveColumn]
    [Column(Db.BiologyDomain.Columns.Position)]
    public int Position { get; set; }

    [Column(Db.BiologyDomain.Columns.Code)]
    public string Code { get; set; }

    [Column(Db.BiologyDomain.Columns.ImageUrl)]
    public string ImageUrl { get; set; }

}