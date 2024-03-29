﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uActivityPub.Data;
using uActivityPub.Models;
using uActivityPub.Notifications;
using uActivityPub.Services.HelperServices;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace uActivityPub.Services.ActivityPubServices;

public class InboxService(
    ILogger<InboxService> logger,
    IUmbracoDatabaseFactory databaseFactory,
    IUmbracoContextAccessor umbracoContextAccessor,
    IEventAggregator eventAggregator,
    IOptions<WebRoutingSettings> webRoutingSettings,
    ISignatureService signatureService,
    ISingedRequestHandler singedRequestHandler)
    : IInboxService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    
    
    public async Task<Activity?> HandleFollow(Activity activity, string signature, string userName, int userId)
    {
        logger.LogInformation("Handling follow request for {Actor}", activity.Actor);
        //todo 1. Check if valid (optional for now)
        var actor = await signatureService.GetActor(activity.Actor);
        
        if (actor == null)
            return null;
        
        var publicPem = actor.PublicKey?.PublicKeyPem;
        var signatureParts = signature.Split(',');

        if (!string.IsNullOrEmpty(publicPem) && signatureParts.Length != 0)
        {
            //we have a pem file and signature header parts
           // var rsa = publicPem.GetRSAFromPem();
        }
        
        //2. Check if already known
        using var database = databaseFactory.CreateDatabase();
        var follow = await database.FirstOrDefaultAsync<ReceivedActivitiesSchema>("SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Actor = @1", "Follow", activity.Actor);

        if (follow != null)
            return null;
        
        //3. save
        var receivedActivity = new ReceivedActivitiesSchema
        {
            Actor = activity.Actor,
            Object = activity.Object as string ?? string.Empty,
            Type = activity.Type
        };

        database.Insert("receivedActivityPubActivities", "Id", true, receivedActivity);
        
        //4. Create response
        var responseActivity =  new Activity
        {
            Id = $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{userName}/{activity.Type}/{receivedActivity.Id}",
            Type = "Accept",
            Actor = activity.Object as string ?? string.Empty,
            Object = activity
        };
        
        var keyInfo = await signatureService.GetPrimaryKeyForUser(userName, userId);

        var response = await singedRequestHandler.SendSingedPost(new Uri(actor.Inbox), keyInfo.Rsa, JsonSerializer.Serialize(responseActivity, JsonSerializerOptions), keyInfo.KeyId);
        
        logger.LogInformation("Send {@ResponseActivity} to {@Actor} response is {@Response} with content {Content}", responseActivity, actor, response, await response.Content.ReadAsStringAsync());

        return responseActivity;
    }

    [ExcludeFromCodeCoverage]
    public async Task<ActionResult> HandleCreate(Activity activity, string signature)
    {
        var activityJObject = (JObject) activity.Object;
        
        if(activityJObject["type"]?.ToObject<string>()?.ToLowerInvariant() != "note" || activityJObject["inReplyTo"]?.ToObject<string>() == null)
            return new BadRequestObjectResult("This type of create is not supported");

        var jsonString = activityJObject.ToString(Formatting.None);
        
        var noteObject = JsonSerializer.Deserialize<Note>(jsonString, JsonSerializerOptions);
        if (noteObject == null)
            return new BadRequestObjectResult("Could not parse note");
        
        using var database = databaseFactory.CreateDatabase();
        var reply = await database.FirstOrDefaultAsync<ReceivedActivitiesSchema>("SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Actor = @1 AND [Object] = @2", "Reply", activity.Actor, activity.Id!);

        if (reply != null)
            return new OkResult();

        var receivedActivity = new ReceivedActivitiesSchema
        {
            Actor = activity.Actor,
            Object = activity.Id!,
            Type = "Reply"
        };
        database.Insert("receivedActivityPubActivities", "Id", true, receivedActivity);

        
        var context = umbracoContextAccessor.GetRequiredUmbracoContext();
        var url = noteObject.InReplyTo!.Replace(webRoutingSettings.Value.UmbracoApplicationUrl.TrimEnd('/'), "");
        var inResponseOfContent = context.Content?.GetByRoute(url);
        
        if(inResponseOfContent != null)
            await eventAggregator.PublishAsync(new ActivityPubReplyReceivedNotification(inResponseOfContent.Id, activity.Actor, noteObject.Content));
        else 
            logger.LogWarning("Could not get content Id from: {Id}", noteObject.InReplyTo);
        
        
        return new OkResult();
    }
    
    public async Task<Activity?> HandleUndo(Activity activity, string signature)
    {
        //todo 1. Check if valid (optional for now)
        //var publicPem = (await _signatureService.GetActor(activity.Actor))?.PublicKey?.PublicKeyPem;
        
        //2. Check if requested undo can be done
        var undoJObject = (JObject) activity.Object;
        var undoObject = undoJObject.ToObject<Activity>();

        if (undoObject?.Type != "Follow")
            throw new InvalidOperationException($"Can't undo {undoObject?.Type} at this time");
        
        //It's an unfollow request
        using var database = databaseFactory.CreateDatabase();
        var follow = await database.FirstOrDefaultAsync<ReceivedActivitiesSchema>("SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Actor = @1", "Follow", undoObject.Actor);

        if (follow == null)
            return null;

        //3. Execute undo
        await database.DeleteAsync(follow);
        return null;
        //4. Respond
    }
}