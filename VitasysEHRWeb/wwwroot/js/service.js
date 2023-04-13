let dataTable

$(document).ready(function () {
    let url = window.location.search;
    if (url.includes("archived")) {
        loadDataTable("archived")
    }
    else {
        loadDataTable("active")
    }
})

let loadDataTable = (status) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Service/GetAll?status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Name" },
            { "data": "TreatmentType.Type" },
            {
                "data": "Price",
                "render": (data) => {
                    return $.fn.dataTable.render.number(',', '.', 2).display(data);
                }
            },
            { "data": "DateAdded" },
            {
                "data": "Id",
                "render": (data) => {
                    if (status == 'archived') {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewTreatment(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View Details</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=deleteTreatment('/Clinic/Service/Delete/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                                </ul>
                            </div>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewTreatment(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View Details</div></div></a></li>
                                    <li><a onClick=editTreatment(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archiveTreatment('/Clinic/Service/Archive/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ]
    })
}

let addTreatment = () => {
    let url = "/Clinic/Service/LoadAddForm"

    $("#addTreatmentModalBodyDiv").load(url, function () {
        $("#addTreatmentModal").modal("show");
    })
}

let editTreatment = (id) => {
    let url = `/Clinic/Service/LoadEditForm?id=${id}`

    $("#editTreatmentModalBodyDiv").load(url, function () {
        $("#editTreatmentModal").modal("show");
    })
}

let viewTreatment = (id) => {
    let url = `/Clinic/Service/GetTreatment?id=${id}`

    $("#viewTreatmentModalBodyDiv").load(url, function () {
        $("#viewTreatmentModal").modal("show");
    })
}

let archiveTreatment = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#1A5599',
        cancelButtonColor: ' #373a3c',
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


let deleteTreatment = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "This record will be permanently deleted!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#1A5599',
        cancelButtonColor: ' #373a3c',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
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