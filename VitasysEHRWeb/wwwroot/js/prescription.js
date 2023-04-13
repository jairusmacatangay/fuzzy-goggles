let viewPrescriptionModalLabel = document.getElementById("viewPrescriptionModalLabel");

let dataTable

$(document).ready(function () {
    let url = window.location.search
    let id = document.getElementById("patientId").value

    if (url.includes("archived")) {
        loadDataTable(id, "archived")
    }
    else {
        loadDataTable(id, "active")
    }
})

let loadDataTable = (id, status) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/PatientRecords/Prescription/GetPrescriptions?id=${id}&status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "Drug" },
            { "data": "Dosage" },
            { "data": "Dose" },
            { "data": "Quantity" },
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
                                    <li><a onClick=deletePrescription('/PatientRecords/Prescription/Delete/${data}') class="dropdown-item" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                                </ul>
                            </div>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onclick=viewPrescription(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View</div></div></a></li>
                                    <li><a onclick=viewPrint(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-print me-2"></i></div><div class="col-10">Print</div></div></a></li>
                                    <li><a onclick=loadEditForm(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archivePrescription('/PatientRecords/Prescription/Archive/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ]
    })
}

let archivePrescription = (url) => {
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

let deletePrescription = (url) => {
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

let loadAddForm = () => {
    let url = "/PatientRecords/Prescription/LoadAddForm"

    $("#addPrescriptionModalBodyDiv").load(url, function () {
        $("#prescriptionModal").modal("show");
    })
}

let viewPrescription = (id) => {
    let url = `/PatientRecords/Prescription/GetPrescription?id=${id}`
    
    $("#viewPrescriptionModalBodyDiv").load(url, function () {
        viewPrescriptionModalLabel.innerHTML = "View Prescription Details";
        $("#viewPrescriptionModal").modal("show");
    })
}

let viewPrint = (id) => {
    let url = `/PatientRecords/Prescription/GetPrint?id=${id}&isPrint=false&clinicId=0`

    $("#viewPrescriptionModalBodyDiv").load(url, function () {
        viewPrescriptionModalLabel.innerHTML = "View Prescription";
        $("#viewPrescriptionModal").modal("show");
    })
}

let loadEditForm = (id) => {
    let url = `/PatientRecords/Prescription/LoadEditForm?id=${id}`

    $("#editPrescriptionModalBodyDiv").load(url, function () {
        $("#editPrescriptionModal").modal("show")
    })
}
