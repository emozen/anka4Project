
window.bootstrapModal = {
    show: function (id) {
        var modalElement = document.querySelector(id);
        if (modalElement) {
            var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.show();
        }
    },
    hide: function (id) {
        var modalElement = document.querySelector(id);
        if (modalElement) {
            var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.hide();
        }
    }
};
