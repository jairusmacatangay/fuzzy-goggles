let dataTable;
let subscriptionType = document.getElementById("subscriptionType").value;
let subscriptionIsLockout = document.getElementById("subscriptionIsLockout").value;

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
            "url": `/Clinic/Patient/GetAll?status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": "Id"
            },
            {
                "data": (data, type, dataToSet) => {
                    return `${data.FirstName} ${data.LastName}`
                }
            },
            { "data": "DOB" },
            { "data": "Gender" },
            { "data": "DateAdded" },
            {
                "data": "LastModified",
                "render": (LastModified) => {
                    if (LastModified == null) {
                        return 'N/A'
                    } else {
                        return `${LastModified}`
                    }
                }
            },
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
                                    <li><a onClick=viewPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=deletePatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                                
                                </ul>
                            </div>`
                    } else if (subscriptionType == "Free") {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                    <li><a href="/PatientRecords/Dashboard/Index?id=${data}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-file-medical me-2"></i></div><div class="col-10">Records</div></div></a></li>
                                    <li><a onClick=editPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archivePatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    } else if (subscriptionIsLockout == "true") {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                    <li><a href="/PatientRecords/Dashboard/Index?id=${data}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-file-medical me-2"></i></div><div class="col-10">Records</div></div></a></li>
                                    <li><a onClick=editPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archivePatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                    <li><a href="/PatientRecords/Dashboard/Index?id=${data}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-file-medical me-2"></i></div><div class="col-10">Records</div></div></a></li>
                                    <li><a onClick=editPatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=loadEmailLinkForm(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-user-cog me-2"></i></div><div class="col-10">Portal Access</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archivePatient(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ]
    })
}

let archivePatient = (id) => {
    let url = `/Clinic/Patient/LoadArchivePatientRequestForm?id=${id}`;

    $("#patientModalBodyDiv").load(url, function () {
        document.getElementById('patientModalLabel').innerHTML = 'Archive Patient Request Form';
        $("#patientModal").modal("show");
    });
}

let deletePatient = (id) => {
    let url = `/Clinic/Patient/LoadDeletePatientRequestForm?id=${id}`;

    $("#patientModalBodyDiv").load(url, function () {
        document.getElementById('patientModalLabel').innerHTML = 'Delete Patient Request Form';
        $("#patientModal").modal("show");
    });
}

let viewPatient = (id) => {
    let url = `/Clinic/Patient/GetPatient?id=${id}`
    let myModal = new bootstrap.Modal(document.getElementById('viewPatientModal'))
    let xhr = new XMLHttpRequest()

    xhr.open("GET", url, true)
    xhr.onreadystatechange = () => {
        if (xhr.readyState != 4 || xhr.status != 200) return
        document.getElementById("viewPatientModalBodyDiv").innerHTML = xhr.responseText
        myModal.show()
    }
    xhr.send()
}

let addPatient = () => {
    let url = "/Clinic/Patient/LoadAddForm"

    $("#addPatientModalBodyDiv").load(url, function () {
        $("#addPatientModal").modal("show");
    })
}

let editPatient = (id) => {
    let url = `/Clinic/Patient/LoadEditPatientRequestForm?id=${id}`;

    $("#editPatientModalBodyDiv").load(url, function () {
        $("#editPatientModal").modal("show");
    });
};

let loadEmailLinkForm = (id) => {
    let url = `/Clinic/Patient/LoadEmailLinkForm?id=${id}`;

    $("#emailLinkModalBodyDiv").load(url, () => {
        $("#emailLinkModal").modal("show");
    });
};