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

        var requestedResource = resource.First();

        if (string.IsNullOrEmpty(requestedResource))
            return BadRequest("Can't search for something without resource request");

        var requestParts = requestedResource.Split(":");

        switch (requestParts.First())
        {
            case "acct":
                return GetAccount(requestParts.Last(), requestedResource);
            default:
                return BadRequest($"request type {requestParts.First()} is not supported");
        }
    }

    private ActionResult<WebFingerResponse> GetAccount(string last, string requestedResource)
    {
        var userName = last.Split('@').First().ToLower();

        if (uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserMode)!.Value == "false")
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

        var singleUserModeName = uActivitySettingsService.GetSettings(uActivitySettingKeys.SingleUserModeUserName);

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