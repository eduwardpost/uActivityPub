﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace uActivityPub.Expansions;

[Tree("settings", uActivityPubConstants.Package.TreeName, TreeTitle = uActivityPubConstants.Package.Name, TreeGroup = uActivityPubConstants.Package.Name, SortOrder = 5)]
[PluginController(uActivityPubConstants.Package.Name)]
[ExcludeFromCodeCoverage]
public class SettingsTreeController(
    ILocalizedTextService localizedTextService,
    UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
    IMenuItemCollectionFactory menuItemCollectionFactory,
    IEventAggregator eventAggregator)
    : TreeController(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();
        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        var menu = _menuItemCollectionFactory.Create();

        return menu;
    }
    
    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        var rootResult = base.CreateRootNode(queryStrings);
        if (rootResult.Result is not null)
        {
            return rootResult;
        }

        var root = rootResult.Value ?? throw new InvalidOperationException($"The value of {nameof(rootResult)} is unexpectedly null");

        //set the route
        root.RoutePath = $"{SectionAlias}/{uActivityPubConstants.Package.TreeName}/dashboard";
        // set the icon
        root.Icon = "icon-mastodon-fill";
        // could be set to false for a custom tree with a single node.
        root.HasChildren = false;
        //url for menu
        root.MenuUrl = null;

        return root;
    }
}