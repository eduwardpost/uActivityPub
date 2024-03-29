using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uActivityPub.Data.Migrations;

[ExcludeFromCodeCoverage]
public class AddUserKeysTable : MigrationBase
{
    public AddUserKeysTable(IMigrationContext context) : base(context)
    {
    }
    protected override void Migrate()
    {
        Logger.LogDebug("Running migration {MigrationStep}", "AddUserKeysTable");

        // Lots of methods available in the MigrationBase class - discover with this.
        if (TableExists("userKeys"))
        {
            Create.Table<UserKeysSchema>().Do();
        }
        else
        {
            Logger.LogDebug("The database table {DbTable} already exists, skipping", "UserKeys");
        }
    }

       
}