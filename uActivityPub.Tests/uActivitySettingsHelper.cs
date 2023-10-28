using System.Collections.Generic;
using uActivityPub.Data;
using uActivityPub.Helpers;

namespace uActivityPub.Tests;

public static class uActivitySettingsHelper
{
   public static List<uActivitySettings> GetSettings()
   {
      return new List<uActivitySettings>
      {
         new uActivitySettings
         {
            Id = 1,
            Key = uActivitySettingKeys.ContentTypeAlias,
            Value = "article"
         },
         new uActivitySettings
         {
            Id = 2,
            Key = uActivitySettingKeys.ListContentTypeAlias,
            Value = "articleList"
         },
         new uActivitySettings
         {
            Id = 3,
            Key = uActivitySettingKeys.UserNameContentAlias,
            Value = "author"
         },
         new uActivitySettings
         {
            Id = 4,
            Key = uActivitySettingKeys.SingleUserMode,
            Value = "false"
         },
         new uActivitySettings
         {
            Id = 4,
            Key = uActivitySettingKeys.SingleUserModeUserName,
            Value = "uActivityPub"
         }
      };
   }
}