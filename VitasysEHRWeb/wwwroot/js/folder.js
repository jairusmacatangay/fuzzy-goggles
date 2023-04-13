let dataTable

$(document).ready(function () {
    let url = window.location.search;
    let id = document.getElementById("patientId").value;
    loadDataTable(id);

})

let loadDataTable = (id) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/PatientRecords/Document/GetAllFolders?id=${id}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": (data, type, dataToSet) => {
                    var folderName = data.Name.substring(0, data.Name.length - 33);
                    var encodeName = data.Name.replace(/ /g, '%20');
                    return `<a onClick=moveToFolder('${encodeName}') href="#" style="text-decoration:none"><i class="fas fa-folder"></i> ${folderName}</a>`
                }
            },
            { "data": "FolderType.Type" },
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
                    return `
                        <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a onClick=loadEditFolderForm('${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a onClick=deleteFolder('/PatientRecords/Document/DeleteFolder/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                            </ul>
                        </div>`
                }
            }
        ],
        "language": {
            "emptyTable": "No folders to be displayed. Create now!"
        }
    })
}

let deleteFolder = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "This folder will be permanently deleted!",
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

let loadAddFolderForm = () => {
    let url = `/PatientRecords/Document/LoadAddFolderForm`

    $("#addFolderModalBodyDiv").load(url, function () {
        $("#addFolderModal").modal("show");
    })
}

let loadEditFolderForm = (id) => {
    let url = `/PatientRecords/Document/LoadEditFolderForm?id=${id}`

    $("#editFolderModalBodyDiv").load(url, function () {
        $("#editFolderModal").modal("show")
    })
}

let moveToFolder = (name) => {
    let url = `/PatientRecords/Document/Folder?name=${name}`
    window.location.href = url;
}
