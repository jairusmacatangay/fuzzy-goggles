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
    const name = new URLSearchParams(window.location.search).get("name");
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/PatientRecords/Document/GetAllDocuments?name=${name}&status=${status}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": (data, type, dataToSet) => {
                    return `<a onClick=viewDocument(${data.Id}) href="#" style="text-decoration:none"><i class="fas fa-file"></i> ${data.Name}</a>`
                }
            },
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
                                <ul class="dropdown-menu">
                                    <li><a onClick=deleteDocument('/PatientRecords/Document/DeleteDocument/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                                </ul>
                            </div>`
                    } else {
                        return `
                            <div class="dropdown">
                                <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a onClick=shareDocument('/PatientRecords/Document/ShareDocument/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-share-square"></i></div><div class="col-10">Share</div></div></a></li>
                                    <li><a onClick=loadEditDocumentForm(${data}) class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Edit</div></div></a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a onClick=archiveDocument('/PatientRecords/Document/ArchiveDocument/${data}') class="dropdown-item text-muted" href="#"><div class="row"><div class="col-2"><i class="fas fa-archive me-2"></i></div><div class="col-10">Archive</div></div></a></li>
                                </ul>
                            </div>`
                    }
                }
            }
        ],
        "language": {
            "emptyTable": "No documents to be displayed. Upload now!"
        }
    })
}

let shareDocument = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "The patient will be able to see this file!",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#1A5599',
        cancelButtonColor: '#373a3c',
        confirmButtonText: 'Yes, share it!'
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

let archiveDocument = (url) => {
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

let deleteDocument = (url) => {
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

let loadAddDocumentForm = (id) => {
    let url = `/PatientRecords/Document/LoadAddDocumentForm?id=${id}`

    $("#addDocumentModalBodyDiv").load(url, function () {
        $("#addDocumentModal").modal("show");
    })
}

let loadEditDocumentForm = (id) => {
    let url = `/PatientRecords/Document/LoadEditDocumentForm?id=${id}`

    $("#editDocumentModalBodyDiv").load(url, function () {
        $("#editDocumentModal").modal("show")
    })
}

let viewDocument = (id) => {
    let url = `/PatientRecords/Document/ViewDocument?id=${id}`

    $("#viewDocumentModalBodyDiv").load(url, function () {
        $("#viewDocumentModal").modal("show");
    })
}