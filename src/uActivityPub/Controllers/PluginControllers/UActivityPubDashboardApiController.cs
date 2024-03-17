using Microsoft.AspNetCore.Authorization;
using uActivityPub.Authorization;
using uActivityPub.Data;
using uActivityPub.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace uActivityPub.Controllers.PluginControllers;

[PluginController(uActivityPubConstants.Package.Name)]
[Authorize(Policy = SyncAuthorizationPolicies.TreeAccessUActivityPub)]
public class UActivityPubDashboardApiController : UmbracoAuthorizedJsonController
{
    private readonly IUActivitySettingsService _uActivitySettingsService;

    public UActivityPubDashboardApiController(IUActivitySettingsService uActivitySettingsService)
    {
        _uActivitySettingsService = uActivitySettingsService;
    }
    
    /// <summary>
    ///  Stub - get API used to locate API in umbraco
    /// </summary>
    /// <returns></returns>
    public bool GetApi() => true;

    
    public IEnumerable<UActivitySettings>? GetSettings()
    {
        return _uActivitySettingsService.GetAllSettings();
    }
}