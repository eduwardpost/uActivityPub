# Listening to Replies

uActivityPub has the ability to receive and process notes that have a reply to one of the Create Notes it made.
In ActivityPub you can Create a new Note and add an `inReplyTo` field in the data. This should then point to whatever you're replying to.

uActivityPub listens to the inbox of the Actor for post request with a create type message. `If` the `Create` type has an `Note` as object `and` the `inReplyTo` field is filled it will try to process it.

The sender of the message should put the url of the post that it is replying to as the value of the inReplyTo field. ActivityPub will then store this (for deduplication) and Create A notification within Umbraco.

### Notification

> ActivityPubReplyReceivedNotification record:
> ```C#
> public record ActivityPubReplyReceivedNotification(int ContentId, string Author, string Content) : INotification;
> ```

This notification will be triggered for each reply received. and Contains the following information.

> ActivityPub Reply ReceivedNotification
>
> ContentId: The Id (`int`) of the content Item within umbraco the reply is to
> 
> Content: The content of the reply (`string`) usually/probably in raw HTML from the platform sending it.
> 
> Author: The Actor (`URI` `string`) that has made the reply