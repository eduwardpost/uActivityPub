(function () {
    'use strict';

    function dashboardController($controller, $scope, $timeout, navigationService, eventsService) {

        var vm = this;

        //var _settingsFolder = Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/uActivityPub/settings';

        vm.selectNavigationItem = function (item) {
            eventsService.emit('uactivitypub-dashboard.tab.change', item);
        }

        vm.page = {
            title: 'uActivityPub',
            description: '...',
            navigation: [ ]
        };

        // var uSyncSettings = Umbraco.Sys.ServerVariables.uSync;

        // if (!uSyncSettings.disabledDashboard) {
        //     vm.page.navigation.push({
        //         'name': 'uActivityPub',
        //         'alias': 'uActivityPub',
        //         'icon': 'icon-mastodon-fill',
        //         'view': _settingsFolder + '/default.html',
        //         'active': true
        //     });
        // }

        // vm.page.navigation.push({
        //     'name': 'Settings',
        //     'alias': 'settings',
        //     'icon': 'icon-settings',
        //     'view': _settingsFolder + '/settings.html',
        // });


        $timeout(function () {
            navigationService.syncTree({ tree: "uActivityPub", path: "-1" });
        });
    }

    angular.module('umbraco')
        .controller('uActivityPubSettingsDashboardController', dashboardController);
})();