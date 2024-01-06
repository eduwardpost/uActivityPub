using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace uActivityPub;

// ReSharper disable once InconsistentNaming
public static class uActivityPub
{
    public static string PackageName => "uActivityPub";
}

public class StaticAssetsBoot : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddUActivityPubAssets();
    }
}

public static class USyncStaticAssetsExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IUmbracoBuilder AddUActivityPubAssets(this IUmbracoBuilder builder)
    {
        // don't add if the filter is already there .
        if (builder.ManifestFilters().Has<UActivityPubAssetManifestFilter>())
            return builder;

        // add the package manifest programatically. 
        builder.ManifestFilters().Append<UActivityPubAssetManifestFilter>();

        return builder;
    }
    
    
    // ReSharper disable once MemberCanBePrivate.Global
    internal class UActivityPubAssetManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(UActivityPubAssetManifestFilter).Assembly;
            
            manifests.Add(new PackageManifest
            {
                PackageId = "uActivityPub",
                PackageName = uActivityPub.PackageName,
                Version = assembly.GetName().Version!.ToString(3),
                AllowPackageTelemetry = true,
                BundleOptions = BundleOptions.Default,
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