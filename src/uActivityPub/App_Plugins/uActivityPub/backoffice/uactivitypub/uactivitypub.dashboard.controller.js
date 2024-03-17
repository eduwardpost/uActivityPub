(function () {
    'use strict';

    function dashboardController($controller, $scope, $http, $timeout, navigationService, eventsService, notificationsService) {
        $scope.loaded = false;

        let vm = this;

        vm.settings = {
            singleUserMode: false,
            singleUserModeUserName: '',
            contentTypeAlias: '',
            listContentTypeAlias: '',
            authorName: ''
        };
        
        function loadSettings () {
            let url = Umbraco.Sys.ServerVariables.uActivityPub.uActivityPubService;

            $http({
                method: 'get',
                url: url + 'getsettings'
            }).then(function successCallback(response) {
                console.log(response.data);
                
                vm.settings.singleUserMode = (response.data.find((setting) => setting.Key === 'singleUserMode').Value === "true")
                vm.settings.singleUserModeUserName = response.data.find((setting) => setting.Key === 'singleUserModeUserName').Value
                vm.settings.gravatarEmail = response.data.find((setting) => setting.Key === 'gravatarEmail').Value
                vm.settings.contentTypeAlias = response.data.find((setting) => setting.Key === 'contentTypeAlias').Value
                vm.settings.listContentTypeAlias = response.data.find((setting) => setting.Key === 'listContentTypeAlias').Value
                vm.settings.authorName = response.data.find((setting) => setting.Key === 'authorName').Value
                
                console.log(vm.settings);
                $scope.loaded = true;
            }, function errorCallback(response) {
                notificationsService.error("Error", "Could not load settings: " + response.status);
            });
        }
        
        
       

        vm.selectNavigationItem = function (item) {
            eventsService.emit('uactivitypub-dashboard.tab.change', item);
        }

        vm.page = {
            title: 'uActivityPub',
            description: 'Settings for the uActivityPub package',
            navigation: []
        };


        $timeout(function () {
            navigationService.syncTree({tree: "uActivityPub", path: "-1"});
        });

        
        loadSettings();
        
    }
    

    angular.module('umbraco')
        .controller('uActivityPubSettingsDashboardController', dashboardController);
})();