app.controller('HomeController', ['$scope', 'releaseNotesService', 'releaseNotesFormattingService', 'busyIndicatorService', function ($scope, releaseNotesService, releaseNotesFormattingService, busyIndicatorService) {

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

                $scope.releaseNotes = releaseNotesFormattingService.formatReleaseNotes(data);

                busyIndicatorService.hide();
            });
    }
}]);