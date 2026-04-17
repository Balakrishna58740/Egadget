<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Orders.aspx.cs"
    Inherits="serena.Admin.OrdersPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Archive Transactions | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-orders-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ord-page">
    <div class="ord-shell">
    <!-- Page Header -->
    <div class="mb-12 flex justify-between items-end ord-header ord-hero">
        <div>
            <h2 class="text-3xl font-serif mb-2">Commerce History</h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Monitor and manage client transactions</p>
        </div>
        <div class="text-right ord-top-actions">
             <button type="button" id="btnExportCsv" class="ord-mini-action"><i class="fa-solid fa-file-csv"></i> Export CSV</button>
             <button type="button" id="btnExportPdf" class="ord-mini-action"><i class="fa-solid fa-file-pdf"></i> Export PDF</button>
             <button type="button" id="btnApplyOrder" class="ord-mini-action ord-mini-action-primary hidden"><i class="fa-solid fa-check"></i> Apply Changes</button>
             <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold bg-white px-4 py-2 border border-gray-100 ord-total"><asp:Literal ID="litTotal" runat="server" /> recorded events</span>
        </div>
    </div>

    <div class="space-y-12">
        <!-- Filter Studio -->
        <div class="bg-white border border-gray-100 p-8 shadow-sm ord-card ord-filter-card">
            <div class="flex items-center gap-4 mb-8 ord-section-head">
                <i class="fa-solid fa-sliders text-primary"></i>
                <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Transaction Search Logic</h3>
            </div>


            <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-8 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4 ord-alert" EnableViewState="false"></asp:Label>

            <div class="ord-filter-grid">
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2 ord-label">Event</label>
                    <asp:DropDownList ID="ddlEventStatus" runat="server" CssClass="ord-input ord-event-dropdown">
                        <asp:ListItem Text="All Events" Value="" />
                        <asp:ListItem Text="Pending" Value="pending" />
                        <asp:ListItem Text="Accepted" Value="accepted" />
                        <asp:ListItem Text="In Process" Value="inprocess" />
                        <asp:ListItem Text="Delivered" Value="delivered" />
                        <asp:ListItem Text="Canceled" Value="canceled" />
                    </asp:DropDownList>
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2 ord-label">Ref Code</label>
                    <asp:TextBox ID="txtCode" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all ord-input" placeholder="e.g. #7721" />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2 ord-label">Client Signature</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all ord-input" placeholder="Enter name..." />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2 ord-label">Chronicle From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all ord-input" TextMode="Date" />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2 ord-label">Chronicle To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all ord-input" TextMode="Date" />
                </div>
            </div>

            <div class="mt-8 flex gap-4 ord-filter-actions">
                <asp:Button ID="btnFilter" runat="server" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-primary transition-all cursor-pointer ord-btn-search" Text="Execute Filter" OnClick="btnFilter_Click" />
                <asp:Button ID="btnReset" runat="server" CssClass="text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:text-primary transition-all cursor-pointer ord-btn-reset" Text="Reset Studio" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Transaction Grid -->
        <div class="bg-white border border-gray-100 shadow-sm overflow-hidden ord-card ord-table-card">
            <div class="ord-table-head">
                <h3>Event Ledger</h3>
                <p>Drag rows to reorder and use settings for quick actions.</p>
            </div>
            <div id="ordBulkBar" class="ord-bulk-bar hidden" aria-live="polite">
                <span id="ordBulkText">0 events selected</span>
                <div class="ord-bulk-actions">
                    <button type="button" class="ord-mini-action" id="ordBulkDuplicate">Duplicate</button>
                    <button type="button" class="ord-mini-action" id="ordBulkArchive">Archive</button>
                </div>
            </div>
            <div class="overflow-x-auto">
                <table class="w-full text-left ord-table">
                    <thead>
                        <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                            <th class="px-4 py-4 w-20 text-center">
                                <input id="chkAllRows" type="checkbox" aria-label="Select all events" />
                            </th>
                            <th class="px-8 py-4">Ref</th>
                            <th class="px-8 py-4">Status</th>
                            <th class="px-8 py-4">Client</th>
                            <th class="px-8 py-4">Finance</th>
                            <th class="px-8 py-4 text-right">Magnitude</th>
                            <th class="px-8 py-4">Event Date</th>
                            <th class="px-8 py-4 text-right">Settings</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-50" id="ordersTbody">
                        <asp:Literal ID="litRows" runat="server" />
                    </tbody>
                </table>
            </div>
            <!-- Pagination -->
            <div class="px-8 py-6 border-t border-gray-50 bg-off-white/10 ord-pager">
                <asp:Literal ID="pager" runat="server" />
            </div>
        </div>
    </div>
    </div>
    </div>

    <div id="ordToast" class="ord-toast" role="status" aria-live="polite"></div>

    <script src="https://cdn.jsdelivr.net/npm/sortablejs@1.15.2/Sortable.min.js"></script>
    <script>
        (function () {
            var m = document.getElementById('<%= lblMsg.ClientID %>');
            if (m && m.textContent && m.textContent.trim().length > 0) {
                m.classList.remove('hidden');
                m.classList.add('block');
                const isError = m.textContent.toLowerCase().includes('sorry') || m.textContent.toLowerCase().includes('error');
                m.classList.add(isError ? 'bg-red-50' : 'bg-green-50');
                m.classList.add(isError ? 'border-red-500' : 'border-green-500');
                m.classList.add(isError ? 'text-red-700' : 'text-green-700');
            }

            var tbody = document.getElementById('ordersTbody');
            var selectAll = document.getElementById('chkAllRows');
            var bulkBar = document.getElementById('ordBulkBar');
            var bulkText = document.getElementById('ordBulkText');
            var applyBtn = document.getElementById('btnApplyOrder');
            var toast = document.getElementById('ordToast');
            var initialOrder = getCurrentOrder();
            var historyStack = [];
            var draggingOrderId = null;

            function announce(msg) {
                if (!toast) return;
                toast.textContent = msg;
                toast.classList.add('show');
                setTimeout(function () { toast.classList.remove('show'); }, 2500);
            }

            function getCurrentOrder() {
                return Array.prototype.map.call(tbody.querySelectorAll('tr.ord-row'), function (r) {
                    return r.getAttribute('data-order-id');
                });
            }

            function updateApplyVisibility() {
                var now = getCurrentOrder().join(',');
                var was = initialOrder.join(',');
                if (now !== was) applyBtn.classList.remove('hidden');
                else applyBtn.classList.add('hidden');
            }

            function refreshBulkState() {
                var checks = tbody.querySelectorAll('.ord-row-check:checked').length;
                if (checks > 0) {
                    bulkBar.classList.remove('hidden');
                    bulkText.textContent = checks + ' events selected';
                } else {
                    bulkBar.classList.add('hidden');
                    bulkText.textContent = '0 events selected';
                }
            }

            if (selectAll) {
                selectAll.addEventListener('change', function () {
                    Array.prototype.forEach.call(tbody.querySelectorAll('.ord-row-check'), function (c) {
                        c.checked = selectAll.checked;
                    });
                    refreshBulkState();
                });
            }

            tbody.addEventListener('change', function (e) {
                if (e.target.classList.contains('ord-row-check')) refreshBulkState();
            });

            var lastTouchTime = 0;

            function onRowActionTap(e) {
                if (e.type === 'click' && (Date.now() - lastTouchTime) < 500) return;
                var detailsBtn = e.target.closest('.ord-action-details');
                if (detailsBtn) {
                    var row = e.target.closest('tr.ord-row');
                    var details = row ? row.querySelector('.ord-inline-details') : null;
                    if (details) {
                        details.classList.toggle('hidden');
                        var wrap2 = detailsBtn.closest('.ord-settings-wrap');
                        if (wrap2) wrap2.classList.remove('open');
                    }
                    return;
                }

                var qa = e.target.closest('.js-duplicate, .js-archive');
                if (qa) {
                    announce(qa.classList.contains('js-duplicate') ? 'Event duplicated in draft mode.' : 'Event archived.');
                    var wrap3 = qa.closest('.ord-settings-wrap');
                    if (wrap3) wrap3.classList.remove('open');
                }
            }

            tbody.addEventListener('click', onRowActionTap);
            tbody.addEventListener('touchstart', function (e) {
                lastTouchTime = Date.now();
                onRowActionTap(e);
            }, { passive: false });

            if (window.Sortable && tbody && window.innerWidth > 767) {
                new Sortable(tbody, {
                    handle: '.ord-drag-handle',
                    draggable: 'tr.ord-row',
                    animation: 300,
                    ghostClass: 'ord-drag-ghost',
                    chosenClass: 'ord-drag-chosen',
                    onStart: function (evt) {
                        draggingOrderId = evt.item ? evt.item.getAttribute('data-order-id') : null;
                        if (evt.item) evt.item.classList.add('ord-dragging');
                    },
                    onEnd: function (evt) {
                        if (evt.item) evt.item.classList.remove('ord-dragging');
                        historyStack.push(initialOrder.slice());
                        var movedCode = evt.item ? (evt.item.getAttribute('data-code') || 'Order') : 'Order';
                        announce(movedCode + ' moved from position ' + (evt.oldIndex + 1) + ' to ' + (evt.newIndex + 1) + '.');
                        updateApplyVisibility();
                        setTimeout(function () { draggingOrderId = null; }, 50);
                    }
                });
            }

            document.addEventListener('keydown', function (e) {
                if (e.ctrlKey && (e.key === 'z' || e.key === 'Z') && historyStack.length > 0) {
                    e.preventDefault();
                    var prevOrder = historyStack.pop();
                    if (prevOrder && prevOrder.length) {
                        var map = {};
                        Array.prototype.forEach.call(tbody.querySelectorAll('tr.ord-row'), function (r) {
                            map[r.getAttribute('data-order-id')] = r;
                        });
                        prevOrder.forEach(function (id) {
                            if (map[id]) tbody.appendChild(map[id]);
                        });
                        announce('Undo applied.');
                        updateApplyVisibility();
                    }
                    return;
                }

                var handle = document.activeElement;
                if (!handle || !handle.classList || !handle.classList.contains('ord-drag-handle')) return;
                if (!e.altKey || (e.key !== 'ArrowUp' && e.key !== 'ArrowDown')) return;
                e.preventDefault();
                var row = handle.closest('tr.ord-row');
                if (!row) return;
                var prev = row.previousElementSibling;
                var next = row.nextElementSibling;
                if (e.key === 'ArrowUp' && prev && prev.classList.contains('ord-row')) {
                    tbody.insertBefore(row, prev);
                    announce('Order moved up.');
                }
                if (e.key === 'ArrowDown' && next && next.classList.contains('ord-row')) {
                    tbody.insertBefore(next, row);
                    announce('Order moved down.');
                }
                updateApplyVisibility();
            });

            document.getElementById('btnExportCsv').addEventListener('click', function () {
                var url = new URL(window.location.href);
                url.searchParams.set('export', 'csv');
                window.location.href = url.toString();
            });

            document.getElementById('btnExportPdf').addEventListener('click', function () {
                window.print();
            });

            applyBtn.addEventListener('click', function () {
                var payload = { orderIds: getCurrentOrder() };
                fetch('/api/orders/reorder.ashx', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                }).then(function (r) {
                    if (!r.ok) throw new Error('save-failed');
                    return r.json();
                }).then(function () {
                    initialOrder = getCurrentOrder();
                    updateApplyVisibility();
                    announce('Order arrangement applied.');
                }).catch(function () {
                    announce('Unable to apply changes. Please try again.');
                });
            });

            var debounced;
            ['<%= txtCode.ClientID %>', '<%= txtName.ClientID %>'].forEach(function (id) {
                var input = document.getElementById(id);
                if (!input) return;
                input.addEventListener('input', function () {
                    clearTimeout(debounced);
                    debounced = setTimeout(function () {
                        document.getElementById('<%= btnFilter.ClientID %>').click();
                    }, 700);
                });
            });
        })();
    </script>
</asp:Content>
