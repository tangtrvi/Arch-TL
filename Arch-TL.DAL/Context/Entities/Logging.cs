using Arch_TL.DAL.Models;
using Arch_TL.DAL.Context.DbPartials;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Arch_TL.DAL.Context.Entities;

[Table("log_exception")]
public class Logging : BaseEntity
{
    [Key]
    [Column(Db.Logging.Columns.Id)]
    public string Id { get; set; }
    [Column(Db.Logging.Columns.TimeStamp)]
    public DateTime TimeStamp { get; set; }
    [Column(Db.Logging.Columns.Level)]
    public string Level { get; set; }
    [Column(Db.Logging.Columns.Logger)]
    public string Logger { get; set; }
    [Column(Db.Logging.Columns.Callsite)]
    public string Callsite { get; set; }
    [Column(Db.Logging.Columns.Exception)]
    public string Exception { get; set; }
    [Column(Db.Logging.Columns.PostParams)]
    public string PostParams { get; set; }
}