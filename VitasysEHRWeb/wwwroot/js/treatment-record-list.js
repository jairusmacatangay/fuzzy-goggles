let treatmentRecordsDatatable;

$(document).ready(function () {
    let id = document.getElementById("patientId").value;
    let url = window.location.search;

    if (url.includes("archived")) {
        loadDataTable("archived", id)
    }
    else {
        loadDataTable("active", id)
    }
});

let loadDataTable = (status, id) => {
    treatmentRecordsDatatable = $('#treatmentRecordsTable').DataTable({
        "ajax": {
            "url": `/PatientRecords/Treatment/GetAllTreatmentRecords?id=${id}&status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            {
                "data": "InvoiceId",
                "render": (InvoiceId) => {
                    if (InvoiceId == null)
                        return 'Not Tagged'
                    return `${InvoiceId}`
                }
            },
            {
                "data": "Treatment.Name",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "ToothNumbers",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "Dentists",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            { "data": "Quantity" },
            {
                "data": "TotalPrice",
                "render": (data) => {
                    return `&#8369; ${$.fn.dataTable.render.number(',', '.', 2).display(data)}`
                }
            },
            {
                "data": "DateCreated",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "LastModified",
                "render": (LastModified) => {
                    if (LastModified == null)
                        return 'N/A'
                    return `${LastModified}`
                }
            },
            {
                "data": null,
                "render": (data, type, row) => {
                    if (status == 'archived') {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary dropdown-toggle rounded-3" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item pointer text-muted" onclick="deleteTreatmentRecord('/PatientRecords/Treatment/Delete/${data.Id}')"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                            </ul>
                        </div>`
                    } else if (data.DateCreated == null) {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary dropdown-toggle rounded-3" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a href="/PatientRecords/Treatment/Upsert/${data.Id}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a class="dropdown-item pointer text-muted" onclick="archiveTreatmentRecord('/PatientRecords/Treatment/Archive/${data.Id}')"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                            </ul>
                        </div>`
                    } else if (data.InvoiceId == null) {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary dropdown-toggle rounded-3" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item pointer text-muted" onclick="loadTreatment(${data.Id})"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                <li><a href="/PatientRecords/Treatment/Upsert/${data.Id}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a class="dropdown-item pointer text-muted" onclick="archiveTreatmentRecord('/PatientRecords/Treatment/Archive/${data.Id}')"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                            </ul>
                        </div>`
                    } else {
                        return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary dropdown-toggle rounded-3" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item pointer text-muted" onclick="loadTreatment(${data.Id})"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                            </ul>
                        </div>`
                     }
                    
                }
            }
        ]
    });
};

async function loadTreatment(id) {
    let response = await fetch(`/PatientRecords/Treatment/LoadTreatment/${id}`);

    if (response.ok) {
        document.getElementById("treatmentModalBodyDiv").innerHTML = await response.text();

        var treatmentModal = new bootstrap.Modal(document.getElementById('treatmentModal'));
        treatmentModal.show();
    } else {
        toastr.options = { "positionClass": "toast-top-center" }
        toastr.error("Treatment does not exist.");
    }
}

let archiveTreatmentRecord = (url) => {
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
                        treatmentRecordsDatatable.ajax.reload();

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

let deleteTreatmentRecord = (url) => {
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
                        treatmentRecordsDatatable.ajax.reload();

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
