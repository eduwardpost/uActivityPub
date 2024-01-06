using Microsoft.AspNetCore.Authorization;
using uActivityPub.Authorization;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace uActivityPub.Controllers.PluginControllers;

[PluginController("uActivitySync")]
[Authorize(Policy = SyncAuthorizationPolicies.TreeAccessuActivitySync)]
public class UActivitySyncDashboardApiController : UmbracoAuthorizedJsonController
{
    
    /// <summary>
    ///  Stub - get API used to locate API in umbraco
    /// </summary>
    /// <returns></returns>
    public bool GetApi() => true;
}