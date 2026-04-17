(function () {
    function safeParseJson(raw) {
        try {
            return JSON.parse(raw || '[]');
        } catch (e) {
            return [];
        }
    }

    function syncFilterMode() {
        var selected = document.querySelector('input[name$="rblType"]:checked');
        var mode = selected ? selected.value : 'range';
        var range = document.getElementById('grpRange');
        var month = document.getElementById('grpMonth');
        var year = document.getElementById('grpYear');

        if (range && month && year) {
            range.classList.toggle('rp-hidden', mode !== 'range');
            month.classList.toggle('rp-hidden', mode !== 'monthly');
            year.classList.toggle('rp-hidden', mode !== 'yearly');
        }
    }

    function renderCharts() {
        var lineLabels = safeParseJson((document.getElementById('hidLineLabels') || {}).value);
        var lineData = safeParseJson((document.getElementById('hidLineData') || {}).value);
        var paymentLabels = safeParseJson((document.getElementById('hidPaymentLabels') || {}).value);
        var paymentData = safeParseJson((document.getElementById('hidPaymentData') || {}).value);

        if (!window.Chart) return;

        var revenueCanvas = document.getElementById('revenueChart');
        if (revenueCanvas) {
            new Chart(revenueCanvas, {
                type: 'line',
                data: {
                    labels: lineLabels,
                    datasets: [{
                        label: 'Revenue (RS)',
                        data: lineData,
                        borderColor: '#00B074',
                        backgroundColor: 'rgba(0,176,116,0.12)',
                        fill: true,
                        tension: 0.3,
                        pointRadius: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true } }
                }
            });
        }

        var paymentCanvas = document.getElementById('paymentChart');
        if (paymentCanvas) {
            new Chart(paymentCanvas, {
                type: 'doughnut',
                data: {
                    labels: paymentLabels,
                    datasets: [{
                        data: paymentData,
                        backgroundColor: ['#00B074', '#1E3A5F', '#10B981', '#334155', '#94A3B8']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { position: 'bottom' } }
                }
            });
        }
    }

    function initReportsDashboard() {
        document.addEventListener('change', function (e) {
            if (e.target && e.target.name && e.target.name.indexOf('rblType') !== -1) {
                syncFilterMode();
            }
        });

        var monthInput = document.getElementById('txtMonth');
        if (monthInput) monthInput.setAttribute('type', 'month');

        syncFilterMode();
        renderCharts();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initReportsDashboard);
    } else {
        initReportsDashboard();
    }
})();
