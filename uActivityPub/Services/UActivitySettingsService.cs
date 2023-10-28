using uActivityPub.Data;
using uActivityPub.Helpers;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Services;

public class UActivitySettingsService : IUActivitySettingsService
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    public UActivitySettingsService(IUmbracoDatabaseFactory  databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }
    
    public IEnumerable<uActivitySettings>? GetAllSettings()
    {
        var database = _databaseFactory.CreateDatabase();
        var settings = database.Fetch<uActivitySettings>($"SELECT * FROM {uActivitySettingKeys.TableName}");

        return settings;
    }

    public uActivitySettings? GetSettings(string key)
    {
        var settings = GetAllSettings();

        return settings?.FirstOrDefault(s => s.Key == key);
    }
}