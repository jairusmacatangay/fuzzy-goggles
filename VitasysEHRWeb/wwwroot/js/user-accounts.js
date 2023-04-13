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
            "url": `/Admin/Account/GetAll?status=${status}`,
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
            { "data": "ApplicationUser.DOB" },
            { "data": "ApplicationUser.Gender" },
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
                        return `<button onClick=viewEmployee('${data}') class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</button>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=viewUser('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">Details</div></div></a></li>
                                    <li><a onClick=editUser('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ]
    })
}

let viewUser = (id) => {
    let url = `/Admin/Account/GetUser?id=${id}`

    $("#viewUserModalBodyDiv").load(url, function () {
        $("#viewUserModal").modal("show");
    })
}

let editUser = (id) => {
    let url = `/Admin/Account/LoadEditRequestForm?id=${id}`

    $("#editUserModalBodyDiv").load(url, function () {
        $("#editUserModal").modal("show");
    })
}