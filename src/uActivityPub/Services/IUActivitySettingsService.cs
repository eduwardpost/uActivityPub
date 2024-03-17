using uActivityPub.Data;

namespace uActivityPub.Services;

public interface IUActivitySettingsService
{
    IEnumerable<UActivitySettings>? GetAllSettings();
    UActivitySettings? GetSettings(string key);
}