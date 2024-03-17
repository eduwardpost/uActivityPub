using NPoco;
using uActivityPub.Data;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public class SettingSeedHelper(IUmbracoDatabaseFactory databaseFactory)
    : INotificationHandler<UmbracoApplicationStartedNotification>
{
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        var database = databaseFactory.CreateDatabase();

        var settings = database.Fetch<UActivitySettings>($"SELECT * FROM {UActivitySettingKeys.TableName}") 
                       ?? [];

        if (settings.TrueForAll(s => s.Key != UActivitySettingKeys.SingleUserMode))
            AddSingleUserModeSettings(database);        
        
        if (settings.TrueForAll(s => s.Key != UActivitySettingKeys.ContentTypeAlias))
            AddContentTypeAliasSettings(database);
        
        if (settings.TrueForAll(s => s.Key != UActivitySettingKeys.GravatarEmail))
            AddGravatarEmailSetting(database);
    }

    private static void AddSingleUserModeSettings(IDatabase database)
    {
        var singleUserModeSetting = new UActivitySettings()
        {
            Key = UActivitySettingKeys.SingleUserMode,
            Value = "false"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, singleUserModeSetting);
        
        var singleUserModeName = new UActivitySettings()
        {
            Key = UActivitySettingKeys.SingleUserModeUserName,
            Value = "uActivityPub"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, singleUserModeName);
    }
    
    private static void AddContentTypeAliasSettings(IDatabase database)
    {
        var contentTypeAlias = new UActivitySettings
        {
            Key = UActivitySettingKeys.ContentTypeAlias,
            Value = "article"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, contentTypeAlias);        
        
        var listContentTypeAlias = new UActivitySettings
        {
            Key = UActivitySettingKeys.ListContentTypeAlias,
            Value = "articleList"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, listContentTypeAlias);
        
        var userNameContentAlias = new UActivitySettings
        {
            Key = UActivitySettingKeys.UserNameContentAlias,
            Value = "authorName"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, userNameContentAlias);
    }
    
    private static void AddGravatarEmailSetting(IDatabase database)
    {
        var singleUserModeSetting = new UActivitySettings()
        {
            Key = UActivitySettingKeys.GravatarEmail,
            Value = "info@uactivitypub.com"
        };
        
        database.Insert(UActivitySettingKeys.TableName, "Id", true, singleUserModeSetting);
    }
}