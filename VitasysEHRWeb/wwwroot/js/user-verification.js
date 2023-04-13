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
            "url": `/Admin/Account/GetAllUnverified`,
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
            { "data": "ApplicationUser.AdminVerified"},
            {
                "data": "ApplicationUser.Id",
                "render": (data) => {
                    return `<button onClick=viewUser('${data}') class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</button>`

                }
            }
        ]
    })
}

let viewUser = (id) => {
    let url = `/Admin/Account/GetUnverifiedUser?id=${id}`

    $("#viewUserModalBodyDiv").load(url, function () {
        $("#viewUserModal").modal("show");
    })
}