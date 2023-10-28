using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uActivityPub.Data.Migrations;

public class AddUActivityTable : MigrationBase
{
    public AddUActivityTable(IMigrationContext context) : base(context)
    {
    }
    protected override void Migrate()
    {
        Logger.LogDebug("Running migration {MigrationStep}", "AddUActivityTable");

        // Lots of methods available in the MigrationBase class - discover with this.
        if (TableExists("uActivitySettings") == false)
        {
            Create.Table<ReceivedActivitiesSchema>().Do();
        }
        else
        {
            Logger.LogDebug("The database table {DbTable} already exists, skipping", "uActivitySettings");
        }
    }

       
}