using Umbraco.Cms.Core.Notifications;

namespace uActivityPub.Notifications;

public record ActivityPubReplyReceivedNotification(int ContentId, string Author, string Content) : INotification;