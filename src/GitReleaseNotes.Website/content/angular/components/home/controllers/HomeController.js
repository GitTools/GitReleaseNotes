app.controller('HomeController', ['$scope', 'releaseNotesService', function ($scope, releaseNotesService) {

    $scope.repositoryUrl = '';
    $scope.repositoryBranch = '';
    $scope.issueTrackerUrl = '';
    $scope.issueTrackerProjectId = '';

    $scope.releaseNotes = '';

    $scope.useExampleValues = function() {
        $scope.repositoryUrl = "https://github.com/catel/catel";
        $scope.repositoryBranch = "develop";
        $scope.issueTrackerUrl = "https://catelproject.atlassian.net";
        $scope.issueTrackerProjectId = "CTL";
    };

    $scope.generateReleaseNotes = function() {
        releaseNotesService.generateReleaseNotes($scope.repositoryUrl, $scope.repositoryBranch, $scope.issueTrackerUrl, $scope.issueTrackerProjectId)
        .then(function (data) {
            $scope.releaseNotes = data.data;
        });
    }
}]);