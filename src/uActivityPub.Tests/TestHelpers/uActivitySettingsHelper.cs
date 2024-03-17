using System.Collections.Generic;
using uActivityPub.Data;
using uActivityPub.Helpers;

namespace uActivityPub.Tests.TestHelpers;

public static class UActivitySettingsHelper
{
   public static List<uActivitySettings> GetSettings()
   {
      return
      [
         new uActivitySettings()
         {
            Id = 1,
            Key = uActivitySettingKeys.ContentTypeAlias,
            Value = "article"
         },

         new uActivitySettings()
         {
            Id = 2,
            Key = uActivitySettingKeys.ListContentTypeAlias,
            Value = "articleList"
         },

         new uActivitySettings()
         {
            Id = 3,
            Key = uActivitySettingKeys.UserNameContentAlias,
            Value = "author"
         },

         new uActivitySettings()
         {
            Id = 4,
            Key = uActivitySettingKeys.SingleUserMode,
            Value = "false"
         },

         new uActivitySettings()
         {
            Id = 5,
            Key = uActivitySettingKeys.SingleUserModeUserName,
            Value = "uActivityPub"
         },

         new uActivitySettings()
         {
            Id = 6,
            Key = uActivitySettingKeys.GravatarEmail,
            Value = "info@uactivitypub.com"
         }
      ];
   }
}