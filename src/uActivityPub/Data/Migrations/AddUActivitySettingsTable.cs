using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uActivityPub.Data.Migrations;

public class AddUActivitySettingsTable : MigrationBase
{
    public AddUActivitySettingsTable(IMigrationContext context) : base(context)
    {
    }
    protected override void Migrate()
    {
        Logger.LogDebug("Running migration {MigrationStep}", "AddUActivityTable");

        // Lots of methods available in the MigrationBase class - discover with this.
        if (TableExists("uActivitySettings"))
        {
            Create.Table<UActivitySettings>().Do();
        }
        else
        {
            Logger.LogDebug("The database table {DbTable} already exists, skipping", "uActivitySettings");
        }
    }

       
}