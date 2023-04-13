let dataTable

$(document).ready(function () {
    let url = window.location.search;
    loadDataTable()
})


let loadDataTable = () => {
    dataTable = $('#folderTable').dataTable({
        "ajax": {
            "url": `/PatientPortal/Records/GetAllFolders`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": (data, type, dataToSet) => {
                    var folderName = data.Name.substring(0, data.Name.length - 33);
                    return `${folderName}`
                }
            },
            { "data": "Type" },
            { "data": "ClinicName" },
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
                    return `<a href="/PatientPortal/Records/ViewDocument?documentId=${data}" class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</a>`
                }
            }
        ],
        "language": {
            "emptyTable": "No folders currently exist"
        }
    })
}

let moveToFolder = (name) => {
    let url = `/PatientPortal/Document/Folder?name=${name}`
    window.location.href = url;
}