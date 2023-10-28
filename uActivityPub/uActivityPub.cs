using System.Diagnostics;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace uActivityPub;

public static class uActivityPub
{
    public static string PackageName => "uActivityPub";
}

public class StaticAssetsBoot : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduActivityPubAssets();
    }
}

public static class uSyncStaticAssetsExtensions
{
    public static IUmbracoBuilder AdduActivityPubAssets(this IUmbracoBuilder builder)
    {
        // don't add if the filter is already there .
        if (builder.ManifestFilters().Has<uActivityPubAssetManifestFilter>())
            return builder;

        // add the package manifest programatically. 
        builder.ManifestFilters().Append<uActivityPubAssetManifestFilter>();

        return builder;
    }
    
    internal class uActivityPubAssetManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(uActivityPubAssetManifestFilter).Assembly;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            //string version = fileVersionInfo.ProductVersion;
            
            manifests.Add(new PackageManifest
            {
                //PackageId = "uActivityPub",
                PackageName = uActivityPub.PackageName,
                Version = assembly.GetName().Version.ToString(3),
                AllowPackageTelemetry = true,
                BundleOptions = BundleOptions.None,
                // Scripts = new[]
                // {
                //     $"{uSyncConstants.Package.PluginPath}/usync.{version}.min.js"
                // },
                // Stylesheets = new[]
                // {
                //     $"{uSyncConstants.Package.PluginPath}/usync.{version}.min.css"
                // }
            });
        }
    }
}