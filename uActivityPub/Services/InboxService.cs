﻿using System.Text.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Serilog;
using uActivityPub.Data;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence;

namespace uActivityPub.Services;

public class InboxService : IInboxService
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;
    private readonly ISignatureService _signatureService;
    private readonly ISingedRequestHandler _singedRequestHandler;

    public InboxService(
        IUmbracoDatabaseFactory  databaseFactory, 
        IOptions<WebRoutingSettings> webRoutingSettings,
        ISignatureService signatureService,
        ISingedRequestHandler singedRequestHandler)
    {
        _databaseFactory = databaseFactory;
        _webRoutingSettings = webRoutingSettings;
        _signatureService = signatureService;
        _singedRequestHandler = singedRequestHandler;
    }
    
    public async Task<Activity?> HandleFollow(Activity activity, string signature, IUser user)
    {
        Log.Information("Handling follow request for {Actor}. with activity {@Activity}", activity.Actor, activity);
        //todo 1. Check if valid (optional for now)
        var actor = await _signatureService.GetActor(activity.Actor);
        
        if (actor == null)
            return null;
        
        var publicPem = actor.PublicKey?.PublicKeyPem;
        var signatureParts = signature.Split(',');

        if (!string.IsNullOrEmpty(publicPem) && signatureParts.Any())
        {
            //we have a pem file and signature header parts
           // var rsa = publicPem.GetRSAFromPem();
        }
        
        //2. Check if already known
        using var database = _databaseFactory.CreateDatabase();
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
            Id = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}/{activity.Type}/{receivedActivity.Id}",
            Type = "Accept",
            Actor = activity.Object as string ?? string.Empty,
            Object = activity
        };
        var keyInfo = await _signatureService.GetPrimaryKeyForUser(user);

        var response = await _singedRequestHandler.SendSingedPost(new Uri(actor.Inbox), keyInfo.Rsa, JsonSerializer.Serialize(responseActivity, new JsonSerializerOptions()
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }), keyInfo.KeyId);
        
        Log.Information("Send {@ResponseActivity} to {@Actor} response is {@Response} with content {Content}", responseActivity, actor, response, await response.Content.ReadAsStringAsync());

        return responseActivity;
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
        using var database = _databaseFactory.CreateDatabase();
        var follow = await database.FirstOrDefaultAsync<ReceivedActivitiesSchema>("SELECT * FROM receivedActivityPubActivities WHERE Type = @0 AND Actor = @1", "Follow", undoObject.Actor);

        if (follow == null)
            return null;

        //3. Execute undo
        await database.DeleteAsync(follow);
        return null;
        //4. Respond
    }
}