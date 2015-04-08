app.service('releaseNotesService', ['$http', '$q', function ($http, $q) {

    this.generateReleaseNotes = function (repositoryUrl, repositoryBranch, issueTrackerUrl, issueTrackerProjectId) {

        var deferred = $q.defer();

        $http.post('/api/releasenotes/generate', {
            repositoryUrl: repositoryUrl,
            repositoryBranch: repositoryBranch,
            issueTrackerUrl: issueTrackerUrl, 
            issueTrackerProjectId: issueTrackerProjectId
            })
            .success(function (data) {
                deferred.resolve(data);
            })
            .error(function (data) {
                deferred.reject(data);
            });

        return deferred.promise;
    };

}]);