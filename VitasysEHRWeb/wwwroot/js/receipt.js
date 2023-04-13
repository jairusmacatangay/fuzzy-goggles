let dataTable

$(document).ready(function () {
    loadDataTable()
})


let loadDataTable = () => {
    dataTable = $('#receiptsTable').DataTable({
        "ajax": {
            "url": `/PatientPortal/Receipt/GetAll`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            {
                "data": "InvoiceDate",
                "render": (InvoiceDate) => {
                    if (InvoiceDate == null) {
                        return 'N/A'
                    } else {
                        return `${InvoiceDate}`
                    }
                }
            },
            { "data": "Clinic" },
            { "data": "PaymentStatus" },
            {
                "data": "TotalAmount",
                "render": (data) => {
                    return `&#8369; ${$.fn.dataTable.render.number(',', '.', 2).display(data)}`
                }
            },
            {
                "data": "Id",
                "render": function (data) {
                    return `<a href="/PatientPortal/Receipt/ViewReceipt?receiptId=${data}" class="btn btn-outline-primary"><i class="fas fa-search-plus me-2"></i>View</a>`
                }
            }
        ]
    })
}

