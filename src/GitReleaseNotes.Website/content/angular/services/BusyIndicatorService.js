app.service('busyIndicatorService', [function () {

    var pleaseWaitDiv = $('<div class="modal hide" id="pleaseWaitDialog" data-backdrop="static" data-keyboard="false"><div class="modal-header"><h1>Processing...</h1></div><div class="modal-body"><div class="progress"><div class="progress-bar progress-striped active" role="progressbar" aria-valuenow="100" aria-valuemax="100" style="width: 100%;"></div></div></div></div>');

    this.show = function() {
        pleaseWaitDiv.modal();
    }

    this.hide = function() {
        pleaseWaitDiv.modal('hide');
    }
}]);