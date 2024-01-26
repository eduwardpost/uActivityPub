using uActivityPub.Models;
using Umbraco.Cms.Core.Models;

namespace uActivityPub.Services;

public interface IActivityHelper
{
    Activity GetActivityFromContent(IContent content, string actor);
}