let dataTable

$(document).ready(function () {
    let url = window.location.search;

    if (url.includes("Archived"))
        loadDataTable("Archived")
    else if (url.includes("Paid"))
        loadDataTable("Paid")
    else if (url.includes("Pending"))
        loadDataTable("Pending")
    else if (url.includes("Created"))
        loadDataTable("Created")
    else if (url.includes("Draft"))
        loadDataTable("Draft")
    else
        loadDataTable("Active")
})

let loadDataTable = (status) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Invoice/GetAllInvoice?status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "PatientName" },
            {
                "data": "InvoiceDate",
                "render": (InvoiceDate) => {
                    if (InvoiceDate == null) {
                        return 'N/A'
                    } else {
                        return `${InvoiceDate}`
                    }
                }
            },
            {
                "data": "InvoiceStatus",
                "render": (data) => {
                    if (data === "Created") {
                        return `<span class="badge rounded-pill bg-success shadow-sm">${data}</span>`
                    } else {
                        return `<span class="badge rounded-pill bg-primary shadow-sm">${data}</span>`
                    }
                }
            },
            {
                "data": "PaymentStatus",
                "render": (data) => {
                    if (data === "Paid") {
                        return `<span class="badge rounded-pill bg-success shadow-sm">${data}</span>`
                    } else {
                        return `<span class="badge rounded-pill bg-primary shadow-sm">${data}</span>`
                    }
                }
            },
            {
                "data": "TotalAmount",
                "render": (data) => {
                    return `&#8369; ${$.fn.dataTable.render.number(',', '.', 2).display(data)}`
                }
            },
            {
                "data": null,
                "render": (data, type, row) => {
                    if (data.IsArchived == true) {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a onclick="loadInvoice(${data.Id})" class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a onClick=deleteInvoice('/Clinic/Invoice/Delete/${data.Id}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                            </ul>
                        </div>`
                    } else if (data.PaymentStatus == "Paid") {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a onclick="loadInvoice(${data.Id})" class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li>
                                     <form action="Invoice/Print" method="get">
                                        <input type="hidden" value="${data.Id}" name="id" />
                                        <button type="submit" class="dropdown-item btn btn-link text-muted"><div class="row"><div class="col-2"><i class="fas fa-print me-2"></i></div><div class="col-10">Print</div></div></button>
                                    </form>
                                </li>
                                <li><a href="/Clinic/Invoice/EmailInvoice?id=${data.Id}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-envelope me-2"></i></div><div class="col-10">Email</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a onClick=archiveInvoice('/Clinic/Invoice/Archive/${data.Id}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                            </ul>
                        </div>`
                    } else if (data.InvoiceStatus == "Draft") {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a href="/Clinic/Invoice/Create?invoiceId=${data.Id}" class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a onClick=archiveInvoice('/Clinic/Invoice/Archive/${data.Id}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                            </ul>
                        </div>`
                    } else {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a onclick="loadInvoice(${data.Id})" class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a onclick="loadCollectPaymentForm(${data.Id})" class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-money-bill me-2"></i></div><div class="col-10">Collect Payment</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li>
                                     <form action="Invoice/Print" method="get">
                                        <input type="hidden" value="${data.Id}" name="id" />
                                        <button type="submit" class="dropdown-item btn btn-link text-muted"><div class="row"><div class="col-2"><i class="fas fa-print me-2"></i></div><div class="col-10">Print</div></div></button>
                                    </form>
                                </li>
                                <li><a href="/Clinic/Invoice/EmailInvoice?id=${data.Id}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-envelope me-2"></i></div><div class="col-10">Email</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a onClick=archiveInvoice('/Clinic/Invoice/Archive/${data.Id}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                            </ul>
                        </div>`
                    }
                    
                }
            }
        ]
    })
}

let loadCreateInvoiceForm = () => {
    let url = "/Clinic/Invoice/LoadCreateInvoiceForm";

    const createInvoiceModalBody = document.getElementById("createInvoiceModalBody");

    if (createInvoiceModalBody.innerHTML.trim() == "") {
        $("#createInvoiceModalBody").load(url, function () {
            $("#createInvoiceModal").modal("show");
        });
    } else {
        $("#createInvoiceModal").modal("show");
    }
};

let loadInvoice = (id) => {
    let url = `/Clinic/Invoice/LoadInvoice?id=${id}`;

    $("#viewInvoiceModalBody").load(url, function () {
        $("#viewInvoiceModal").modal("show")
    });
};

let loadCollectPaymentForm = (id) => {
    let url = `/Clinic/Invoice/LoadCollectPaymentForm?id=${id}`;

    $("#collectPaymentModalBody").load(url, function () {
        $("#collectPaymentModal").modal("show");
    });
};

let archiveInvoice = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#1A5599',
        cancelButtonColor: '#373a3c',
        confirmButtonText: 'Yes, archive it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();

                        toastr.options = { "positionClass": "toast-top-center" }
                        toastr.success(data.message);
                    }
                    else {
                        toastr.options = { "positionClass": "toast-top-center" }
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

let deleteInvoice = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "This record will be permanently deleted!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#1A5599',
        cancelButtonColor: '#373a3c',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();

                        toastr.options = { "positionClass": "toast-top-center" };
                        toastr.success(data.message);
                    }
                    else {
                        toastr.options = { "positionClass": "toast-top-center" };
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}