let dataTable

$(document).ready(function () {
    loadDataTable()
});

let loadDataTable = () => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": "/Admin/Auditlog/GetAuditLogs",
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "Username" },
            {
                "data": "Clinic",
                "render": (Clinic) => {
                    if (Clinic != null)
                        return Clinic
                    return 'N/A'
                }
            },
            { "data": "DateAdded" },
            { "data": "ActivityType" },
            { "data": "Description" },
            { "data": "Device" },
        ]
    })
}
