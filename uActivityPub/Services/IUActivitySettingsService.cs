using uActivityPub.Data;

namespace uActivityPub.Services;

public interface IUActivitySettingsService
{
    IEnumerable<uActivitySettings>? GetAllSettings();
    uActivitySettings? GetSettings(string key);
}