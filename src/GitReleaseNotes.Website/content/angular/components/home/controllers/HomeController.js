app.controller('HomeController', ['$scope', 'releaseNotesService', 'busyIndicatorService', function ($scope, releaseNotesService, busyIndicatorService) {

    $scope.repositoryUrl = '';
    $scope.repositoryBranch = '';
    $scope.issueTrackerUrl = '';
    $scope.issueTrackerProjectId = '';

    $scope.releaseNotes = '';

    $scope.useExampleValues = function () {
        $scope.repositoryUrl = 'https://github.com/catel/catel';
        $scope.repositoryBranch = 'develop';
        $scope.issueTrackerUrl = 'https://catelproject.atlassian.net';
        $scope.issueTrackerProjectId = 'CTL';
    };

    $scope.generateReleaseNotes = function () {

        busyIndicatorService.show();

        releaseNotesService.generateReleaseNotes($scope.repositoryUrl, $scope.repositoryBranch, $scope.issueTrackerUrl, $scope.issueTrackerProjectId)
            .then(function (data) {

                // TODO: ultimately, this should go into a service

                var fullReleaseNotes = '';

                for (var i = 0; i < data.releases.length; i++) {
                    var release = data.releases[i];
                    fullReleaseNotes += release.releaseName + ' (' + release.when + ')' + '\n';

                    for (var j = 0; j < release.releaseNoteItems.length; j++) {
                        var releaseNoteItem = release.releaseNoteItems[j];
                        fullReleaseNotes += '[' + releaseNoteItem.issueNumber + '] ' + releaseNoteItem.title + '\n';
                    }

                    fullReleaseNotes += '\n';
                }

                $scope.releaseNotes = fullReleaseNotes;

                busyIndicatorService.hide();
            });
    }
}]);