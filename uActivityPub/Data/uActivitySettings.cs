using NPoco;
using uActivityPub.Helpers;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uActivityPub.Data;

// ReSharper disable once InconsistentNaming
[TableName(uActivitySettingKeys.TableName)]
[PrimaryKey("Id", AutoIncrement = false)]
[ExplicitColumns]
public class uActivitySettings
{
    [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
    [Column("UserId")]
    public int Id { get; set; }
    
    [Column("Key")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Key { get; set; } = default!;

    [Column("Value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = default!;
}