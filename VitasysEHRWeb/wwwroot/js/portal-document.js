let dataTable

$(document).ready(function () {
        loadDataTable()
})

let loadDataTable = () => {
    const id = new URLSearchParams(window.location.search).get("documentId");
    dataTable = $('#folderTable').DataTable({
        "ajax": {
            "url": `/PatientPortal/Records/GetAllDocuments?Id=${id}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Name" },
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
                "render": (data, type, dataToSet) => {
                    return `<a onClick=viewDocument(${data}) href="#" class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</a>`
                    }
                }
            
        ],
        "language": {
            "emptyTable": "No documents available."
        }
    })
}

let viewDocument = (id) => {
    let url = `/PatientPortal/Records/ViewImage?Id=${id}`

    $("#viewDocumentModalBodyDiv").load(url, function () {
        $("#viewDocumentModal").modal("show");
    })
}