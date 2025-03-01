
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

window.renderDoughnutChart = (canvasId, chartData) => {
    const ctx = document.getElementById(canvasId).getContext('2d');
    new Chart(ctx, {
        type: 'doughnut',
        data: chartData,
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Gelir ve Gider Dağılımı'
                }
            }
        }
    });
};

Chart.register(ChartDataLabels);

document.addEventListener('DOMContentLoaded', function () {
    window.renderDoughnutChart = (canvasId, chartData) => {
        const ctx = document.getElementById(canvasId).getContext('2d');
        new Chart(ctx, {
            type: 'doughnut',
            data: chartData,
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Gelir ve Gider Dağılımı'
                    }
                }
            }
        });
    };
});



function resetInputFile(inputElement) {
    inputElement.value = null;
}
