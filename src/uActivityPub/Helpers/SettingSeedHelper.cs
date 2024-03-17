using NPoco;
using uActivityPub.Data;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public class SettingSeedHelper : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    public SettingSeedHelper(IUmbracoDatabaseFactory  databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }
    
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        var database = _databaseFactory.CreateDatabase();

        var settings = database.Fetch<uActivitySettings>($"SELECT * FROM {uActivitySettingKeys.TableName}") 
                       ?? new List<uActivitySettings>();

        if (settings.All(s => s.Key != uActivitySettingKeys.SingleUserMode))
            AddSingleUserModeSettings(database);        
        
        if (settings.All(s => s.Key != uActivitySettingKeys.ContentTypeAlias))
            AddContentTypeAliasSettings(database);
        
        if (settings.All(s => s.Key != uActivitySettingKeys.GravatarEmail))
            AddGravatarEmailSetting(database);
    }

    private static void AddSingleUserModeSettings(IDatabase database)
    {
        var singleUserModeSetting = new uActivitySettings()
        {
            Key = uActivitySettingKeys.SingleUserMode,
            Value = "false"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, singleUserModeSetting);
        
        var singleUserModeName = new uActivitySettings()
        {
            Key = uActivitySettingKeys.SingleUserModeUserName,
            Value = "uActivityPub"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, singleUserModeName);
    }
    
    private static void AddContentTypeAliasSettings(IDatabase database)
    {
        var contentTypeAlias = new uActivitySettings
        {
            Key = uActivitySettingKeys.ContentTypeAlias,
            Value = "article"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, contentTypeAlias);        
        
        var listContentTypeAlias = new uActivitySettings
        {
            Key = uActivitySettingKeys.ListContentTypeAlias,
            Value = "articleList"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, listContentTypeAlias);
        
        var userNameContentAlias = new uActivitySettings
        {
            Key = uActivitySettingKeys.UserNameContentAlias,
            Value = "authorName"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, userNameContentAlias);
    }
    
    private static void AddGravatarEmailSetting(IDatabase database)
    {
        var singleUserModeSetting = new uActivitySettings()
        {
            Key = uActivitySettingKeys.GravatarEmail,
            Value = "info@uactivitypub.com"
        };
        
        database.Insert(uActivitySettingKeys.TableName, "Id", true, singleUserModeSetting);
    }
}