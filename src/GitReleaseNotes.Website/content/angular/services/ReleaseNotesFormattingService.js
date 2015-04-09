app.service('releaseNotesFormattingService', [function () {

    this.formatReleaseNotes = function (releaseNotes) {

        var fullReleaseNotes = '';

        for (var i = 0; i < releaseNotes.releases.length; i++) {
            var release = releaseNotes.releases[i];

            if (release.releaseName) {
                fullReleaseNotes += release.releaseName + ' (' + release.when + ')' + '\n';
            } else {
                fullReleaseNotes += '#vNext' + '\n';
            }

            for (var j = 0; j < release.releaseNoteItems.length; j++) {
                var releaseNoteItem = release.releaseNoteItems[j];
                fullReleaseNotes += '[' + releaseNoteItem.issueNumber + '] ' + releaseNoteItem.title + '\n';
            }

            fullReleaseNotes += '\n';
        }

        return fullReleaseNotes;
    };

}]);