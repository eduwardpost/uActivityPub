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
    
    public IEnumerable<UActivitySettings>? GetAllSettings()
    {
        var database = _databaseFactory.CreateDatabase();
        var settings = database.Fetch<UActivitySettings>($"SELECT * FROM {UActivitySettingKeys.TableName}");

        return settings;
    }

    public UActivitySettings? GetSettings(string key)
    {
        var settings = GetAllSettings();

        return settings?.FirstOrDefault(s => s.Key == key);
    }
}