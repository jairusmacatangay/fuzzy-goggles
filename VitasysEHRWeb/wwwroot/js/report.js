var dataTable;

function getDetails() {
    var value = document.getElementById("ReportId");
    var getVal = value.options[value.selectedIndex].value;
    var input;

    $("#patient").remove();
    $("#user").remove();

    switch (getVal) {
        case "1":
            resetTable();
            loadDataTableForPatients();
            break;
        case "2":
        case "3":
            resetTable();
            input = $('<input type="number" name="patient" class="form-control" id="patient" placeholder="Input Patient ID here" />');
            $("#inputContainer").append(input);
            $("#patient").on('keyup', function (e) {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    var inputVal = document.getElementById("patient").value;
                    if (getVal == 2) {
                        loadDataTableForTreatmentsPerformed(inputVal);
                    } else {
                        loadDataTableForInvoices(inputVal);
                    }
                }
            })
            break;
        case "4":
            resetTable();
            input = $('<input type="text" name="user" class="form-control" id="user" placeholder="Input User ID here" />');
            $("#inputContainer").append(input);
            $("#user").on('keyup', function (e) {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    var inputVal = document.getElementById("user").value;
                    loadDataTableForAuditLogs(inputVal);
                }
            })
            break;
        default:
            break;
    }
}

let loadDataTableForAuditLogs = (id) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Report/GetAllAuditLogs?id=${id}`,
            "dataSrc": ""
        },
        "dom": "<'row'<'col-sm-12 col-md-2'l><'col-sm-12 col-md-2'B><'col-sm-12 col-md-8'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "buttons": [
            {
                "extend": "print",
                "text": '<i class="fas fa-print"></i> Print',
                "title": "Audit Logs - VitaSys EHR",
                "autoPrint": "false"
            }
        ],
        "columns": [
            { "data": "ActivityType", "title": "Activity Type" },
            { "data": "Description", "title": "Description"  },
            { "data": "Device", "title": "Device"  },
            { "data": "DateAdded", "title": "Date Added"  }
        ],
        "drawCallback": function (settings) {
            this.dataTable.button(0).disable(settings.json.recordsTotal > 0);
        }
    })
    applyHeaderStyle();
}

let loadDataTableForInvoices = (id) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Report/GetAllInvoices?id=${id}`,
            "dataSrc": ""
        },
        "dom": "<'row'<'col-sm-12 col-md-2'l><'col-sm-12 col-md-2'B><'col-sm-12 col-md-8'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "buttons": [
            {
                "extend": "print",
                "text": '<i class="fas fa-print"></i> Print',
                "title": "Invoices - VitaSys EHR",
                "autoPrint": "false"
            }
        ],
        "columns": [
            { "data": "InvoiceDate", "title": "Invoice Date" },
            { "data": "TotalAmount", "title": "Total Amount" },
            { "data": "AmountPaid", "title": "Amount Paid" },
            { "data": "Change", "title": "Change" },
            { "data": "PaymentStatus.Status", "title": "Payment Status" },
            { "data": "PaymentMethod.Method", "title": "Payment Method" },
            {
                "data": "PaymentDate",
                "render": (PaymentDate) => {
                    if (PaymentDate == "01/01/0001 12:00 AM") {
                        return 'N/A'
                    } else {
                        return `${PaymentDate}`
                    }
                },
                "title": "Payment Date"
            },
            {
                "data": "LastModified",
                "render": (LastModified) => {
                    if (LastModified == "01/01/0001 12:00 AM") {
                        return 'N/A'
                    } else {
                        return `${LastModified}`
                    }
                },
                "title": "Last Modified"
            }
        ]
    })
    applyHeaderStyle();
}

let loadDataTableForTreatmentsPerformed = (id) => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Report/GetAllTreatmentsPerformed?id=${id}`,
            "dataSrc": "",
        },
        "dom": "<'row'<'col-sm-12 col-md-2'l><'col-sm-12 col-md-2'B><'col-sm-12 col-md-8'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "buttons": [
            {
                "extend": "print",
                "text": '<i class="fas fa-print"></i> Print',
                "title": "Treatments Performed - VitaSys EHR",
                "autoPrint": "false"
            }
        ],
        "columns": [
            { "data": "Treatment.Name", "title": "Treatment Name" },
            { "data": "Treatment.TreatmentType.Type", "title": "Treatment Type" },
            { "data": "Treatment.Price", "title": "Price" },
            { "data": "Treatment.DateAdded", "title": "Date Added" },
            {
                "data": "Treatment.LastModified",
                "render": (LastModified) => {
                    if (LastModified == "01/01/0001 12:00 AM") {
                        return 'N/A'
                    } else {
                        return `${LastModified}`
                    }
                },
                "title": "Last Modified"
            }
        ]
    })
    applyHeaderStyle();
}

let loadDataTableForPatients = () => {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": `/Clinic/Report/GetAllPatients`,
            "dataSrc": ""
        },
        "dom": "<'row'<'col-sm-12 col-md-2'l><'col-sm-12 col-md-2'B><'col-sm-12 col-md-8'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        "buttons": [
            {
                "extend": "print",
                "text": '<i class="fas fa-print"></i> Print',
                "title": "Patients in the Clinic - VitaSys EHR",
                "autoPrint": "false" 
            }
        ],
        "columns": [
            {
                "data": (data, type, dataToSet) => {
                    return `${data.LastName}, ${data.FirstName} ${data.MiddleName}`
                },
                "title": "Name"
            },
            { "data": "DOB", "title" : "Birthdate" },
            { "data": "Gender", "title": "Gender" },
            { "data": "MobileNumber", "title": "Mobile Number" },
            { "data": "Address", "title": "Address" },
            { "data": "City", "title": "City" },
            { "data": "Province", "title": "Province" },
            { "data": "ZipCode", "title": "Zip Code" },
            { "data": "DateAdded", "title": "Date Added" },
            {
                "data": "LastModified",
                "render": (LastModified) => {
                    if (LastModified == null) {
                        return 'N/A'
                    } else {
                        return `${LastModified}`
                    }
                },
                "title" : "Last Modified"
            }
        ]
    })
    applyHeaderStyle();
}

function applyHeaderStyle() {
    $('#myTable thead tr th').each(function (_, el) {
        $(el).addClass('bg-primary text-white');
    })
}

function resetTable() {
    if ($.fn.DataTable.isDataTable("#myTable")) {
        $('#myTable').DataTable().clear().destroy();
        $('#myTable').empty();
    }
}