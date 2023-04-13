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
            "url": `/Clinic/Employee/GetAll?status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": (data, type, dataToSet) => {
                    return `${data.ApplicationUser.FirstName} ${data.ApplicationUser.LastName}`
                }
            },
            { "data": "SelectedRoles[,&nbsp]" },
            { "data": "ApplicationUser.DateAdded" },
            {
                "data": "ApplicationUser.LastModified",
                "render": (LastModified) => {
                    if (LastModified == null) {
                        return 'N/A'
                    } else {
                        return `${LastModified}`
                    }
                }
            },
            {
                "data": "ApplicationUser.Id",
                "render": (data) => {
                    if (status == 'archived') {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewEmployee('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View Details</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=deleteEmployee('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                                </ul>
                            </div>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewEmployee('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View Details</div></div></a></li>
                                    <li><a onClick=editEmployee('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archiveEmployee('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ]
    })
}

let addEmployee = () => {
    let url = "/Clinic/Employee/LoadAddForm"

    $("#addEmployeeModalBodyDiv").load(url, function () {
        $("#addEmployeeModal").modal("show");
    })
}

let viewEmployee = (id) => {
    let url = `/Clinic/Employee/GetEmployee?id=${id}`

    $("#viewEmployeeModalBodyDiv").load(url, function () {
        $("#viewEmployeeModal").modal("show");
    })
}

let editEmployee = (id) => {
    let url = `/Clinic/Employee/LoadEditRequestForm?id=${id}`

    $("#editEmployeeModalBodyDiv").load(url, function () {
        $("#editEmployeeModal").modal("show");
    })
}

let archiveEmployee = (id) => {
    let url = `/Clinic/Employee/LoadArchiveEmployeeRequestForm?id=${id}`;

    $("#employeeModalBodyDiv").load(url, function () {
        document.getElementById('employeeModalLabel').innerHTML = 'Archive Employee Request Form';
        $("#employeeModal").modal("show");
    });
}

let deleteEmployee = (id) => {
    let url = `/Clinic/Employee/LoadDeleteEmployeeRequestForm?id=${id}`;

    $("#employeeModalBodyDiv").load(url, function () {
        document.getElementById('employeeModalLabel').innerHTML = 'Delete Employee Request Form';
        $("#employeeModal").modal("show");
    });
}