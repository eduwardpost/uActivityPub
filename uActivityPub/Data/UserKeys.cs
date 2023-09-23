using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uActivityPub.Data;

[TableName("userKeys")]
[PrimaryKey("Id", AutoIncrement = false)]
[ExplicitColumns]
public class UserKeysSchema
{
    [PrimaryKeyColumn(AutoIncrement = false)]
    [Column("UserId")]
    public int Id { get; set; }

    [Column("PrivateKey")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string PrivateKey { get; set; } = default!;

    [Column("PublicKey")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string PublicKey { get; set; } = default!;
}