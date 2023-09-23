using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using uActivityPub.Helpers;
using uActivityPub.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace uActivityPub.Controllers;

[Route("/.well-known/webfinger")]
public class WebfingerController : UmbracoApiController
{
    private readonly IUserService _userService;
    private readonly IOptions<WebRoutingSettings> _webRoutingSettings;

    public WebfingerController(IUserService userService, IOptions<WebRoutingSettings> webRoutingSettings)
    {
        _userService = userService;
        _webRoutingSettings = webRoutingSettings;
    }


    [HttpGet("")]
    public ActionResult<WebFingerResponse> HandleRequest()
    {
        var resource = Request.Query["resource"];

        if (!resource.Any())
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
        var user = _userService.GetAll(0, 100, out _).FirstOrDefault(u => u.ActivityPubUserName() == userName);

        if (user == null)
            return NotFound();

        return Ok(new WebFingerResponse
        {
            Subject = requestedResource,
            Links = new[]
            {
                new WebFingerLink
                {
                    Rel = "self",
                    Href = $"{_webRoutingSettings.Value.UmbracoApplicationUrl}activitypub/actor/{user.ActivityPubUserName()}"
                }
            }
        });
    }
}