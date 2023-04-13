let treatmentRecordsDatatable;

$(document).ready(function () {
    loadDataTable()
})

let loadDataTable = () => {
    treatmentRecordsDatatable = $('#treatmentRecordsTable').DataTable({
        "ajax": {
            "url": `/PatientPortal/Records/GetAllTreatments`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            {
                "data": "InvoiceId",
                "render": (InvoiceId) => {
                    if (InvoiceId == null)
                        return 'Not Tagged'
                    return `${InvoiceId}`
                }
            },
            {
                "data": "TreatmentName",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "ToothNumbers",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "ClinicName",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "Dentists",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            { "data": "Quantity" },
            {
                "data": "Price",
                "render": (data) => {
                    return `&#8369; ${$.fn.dataTable.render.number(',', '.', 2).display(data)}`
                }
            },
            {
                "data": "DateCreated",
                "render": (data) => {
                    if (data == null)
                        return '-'
                    return `${data}`
                }
            },
            {
                "data": "LastModified",
                "render": (data) => {
                    if (data == "")
                        return 'N/A'
                    return `${data}`
                }
            }
     
        ]
    });
};

