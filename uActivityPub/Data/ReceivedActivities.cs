using System.ComponentModel.DataAnnotations.Schema;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uActivityPub.Data;

[TableName("receivedActivityPubActivities")]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public class ReceivedActivitiesSchema
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
    [NPoco.Column("Id")]
    public long Id { get; set; }
    
    [NPoco.Column("Type")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Type { get; set; } = default!;
    
    [NPoco.Column("Actor")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Actor { get; set; } = default!;

    [NPoco.Column("Object")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Object { get; set; } = default!;

}