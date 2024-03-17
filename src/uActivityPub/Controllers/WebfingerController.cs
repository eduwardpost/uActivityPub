using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using uActivityPub.Helpers;
using uActivityPub.Models;
using uActivityPub.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace uActivityPub.Controllers;

[Route("/.well-known/webfinger")]
public class WebfingerController(
    IUserService userService,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IUActivitySettingsService uActivitySettingsService)
    : UmbracoApiController
{
    [HttpGet("")]
    public ActionResult<WebFingerResponse> HandleRequest()
    {
        var resource = Request.Query["resource"];

        if (resource.Count == 0)
            return BadRequest("Can't search for something without resource request");

        var requestedResource = resource[0];

        if (string.IsNullOrEmpty(requestedResource))
            return BadRequest("Can't search for something without resource request");

        var requestParts = requestedResource.Split(":");

        switch (requestParts[0])
        {
            case "acct":
                return GetAccount(requestParts[^1], requestedResource);
            default:
                return BadRequest($"request type {requestParts[0]} is not supported");
        }
    }

    private ActionResult<WebFingerResponse> GetAccount(string last, string requestedResource)
    {
        var userName = last.Split('@')[0].ToLower();

        if (uActivitySettingsService.GetSettings(UActivitySettingKeys.SingleUserMode)!.Value == "false")
        {
            var user = userService.GetAll(0, 100, out _).FirstOrDefault(u => u.ActivityPubUserName() == userName);

            if (user == null)
                return NotFound();

            return Ok(new WebFingerResponse
            {
                Subject = requestedResource,
                Links =
                [
                    new WebFingerLink
                    {
                        Rel = "self",
                        Href =
                            $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}"
                    }
                ]
            });
        }

        var singleUserModeName = uActivitySettingsService.GetSettings(UActivitySettingKeys.SingleUserModeUserName);

        return Ok(new WebFingerResponse
        {
            Subject = requestedResource,
            Links =
            [
                new WebFingerLink
                {
                    Rel = "self",
                    Href =
                        $"{webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{singleUserModeName!.Value.ToLowerInvariant()}"
                }
            ]
        });
    }
}