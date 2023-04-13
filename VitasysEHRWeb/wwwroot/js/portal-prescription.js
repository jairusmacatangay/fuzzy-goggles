let dataTable

$(document).ready(function () {
    loadDataTable()
})


let loadDataTable = () => {
    dataTable = $('#prescriptionTable').DataTable({
        "ajax": {
            "url": `/PatientPortal/Records/GetAllPrescriptions`,
            "dataSrc" : ``
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "ClinicName" },
            { "data": "Drug" },
            { "data": "Dosage" },
            { "data": "Dose" },
            { "data": "Quantity" },
            { "data": "DateAdded" },
            {
                "data": "Id",
                "render": (data) => {
                    return `<a href="/PatientPortal/Records/ViewPrescription?prescriptionId=${data}" class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</a>`
                }
            }

        ]
    })
}
