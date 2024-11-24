using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace uActivityPub;

[ExcludeFromCodeCoverage]
public static class USyncStaticAssetsExtensions
{
    // ReSharper disable once MemberCanBePrivate.Global
    internal class UActivityPubAssetManifestReader : IPackageManifestReader
    {
        public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            return await Task.Run(() =>
            {
                var assembly = typeof(UActivityPubAssetManifestReader).Assembly;
                return new List<PackageManifest>
                {
                    new ()
                    {
                        Id = uActivityPubConstants.Package.Name,
                        Name = uActivityPubConstants.Package.Name,
                        Version = assembly.GetName().Version!.ToString(3),
                        AllowTelemetry = true,
                        Extensions = []
                    }
                };
            });
        }
    }
}