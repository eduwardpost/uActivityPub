using System.Collections.Generic;
using uActivityPub.Data;
using uActivityPub.Helpers;

namespace uActivityPub.Tests.TestHelpers;

public static class UActivitySettingsHelper
{
   public static List<UActivitySettings> GetSettings()
   {
      return
      [
         new UActivitySettings()
         {
            Id = 1,
            Key = UActivitySettingKeys.ContentTypeAlias,
            Value = "article"
         },

         new UActivitySettings()
         {
            Id = 2,
            Key = UActivitySettingKeys.ListContentTypeAlias,
            Value = "articleList"
         },

         new UActivitySettings()
         {
            Id = 3,
            Key = UActivitySettingKeys.UserNameContentAlias,
            Value = "author"
         },

         new UActivitySettings()
         {
            Id = 4,
            Key = UActivitySettingKeys.SingleUserMode,
            Value = "false"
         },

         new UActivitySettings()
         {
            Id = 5,
            Key = UActivitySettingKeys.SingleUserModeUserName,
            Value = "uActivityPub"
         },

         new UActivitySettings()
         {
            Id = 6,
            Key = UActivitySettingKeys.GravatarEmail,
            Value = "info@uactivitypub.com"
         }
      ];
   }
}