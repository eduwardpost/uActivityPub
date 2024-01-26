using System.ComponentModel.DataAnnotations.Schema;
using NPoco;
using uActivityPub.Helpers;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uActivityPub.Data;

// ReSharper disable once InconsistentNaming
[TableName(uActivitySettingKeys.TableName)]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public class uActivitySettings
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
    [NPoco.Column("Id")]
    public long Id { get; set; }
    
    [NPoco.Column("Key")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Key { get; set; } = default!;

    [NPoco.Column("Value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = default!;
}