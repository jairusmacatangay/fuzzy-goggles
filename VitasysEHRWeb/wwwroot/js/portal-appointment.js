let dataTable

$(document).ready(function () {
    loadDataTable()
})

let loadDataTable = () => {
    dataTable = $('#appointmentTable').DataTable({
        "ajax": {
            "url": `/PatientPortal/Appointment/GetAllAppointments`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "AppointmentDate" },
            {
                "data": "AppointmentTime.Id",
                "orderData": [2, 3],
                "visible": false,
                "searchable": false
            },
            { "data": "AppointmentTime.TimeSlot" },
            { "data": "Clinic" },
            {
                "data": "AppointmentStatus",
                "render": (data) => {
                    if (data === 'Approved') {
                        return `<span class="badge rounded-pill bg-success shadow-sm">${data}</span>`
                    } else if (data === 'Pending') {
                        return `<span class="badge rounded-pill bg-primary shadow-sm">${data}</span>`
                    } else {
                        return `<span class="badge rounded-pill bg-danger shadow-sm">${data}</span>`
                    }
                }
            },
            {
                "data": "Id",
                "render": (data) => {
                    return `<a class="btn btn-outline-primary" href="Appointment/View/${data}"><i class="fas fa-search-plus me-2"></i>View</a>`
                }
            }
        ],
        "order": [[1, "desc"], [2, "asc"]]
    })
}