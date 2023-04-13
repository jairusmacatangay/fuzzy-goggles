let dataTable

$(document).ready(function () {
    loadDataTable()
})

let loadDataTable = () => {
    dataTable = $('#appointmentTable').DataTable({
        "ajax": {
            "url": `/Clinic/Appointment/GetAllAppointments`,
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
            { "data": "Patient" },
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
                    return `<button class="btn btn-outline-primary" onclick="loadAppointment(${data})"><i class="fas fa-search-plus me-2"></i>View</button>`
                }
            }
        ],
        "order": [[1, "desc"], [2, "asc"]]
    })
}

let loadAppointment = (id) => {
    let url = `/Clinic/Appointment/LoadAppointment?id=${id}`;

    $("#viewAppointmentModalBody").load(url, () => {
        $("#viewAppointmentModal").modal("show");
    });
};

function denyAppt() {
    const notes = document.getElementById("apptNotes");
    const status = document.getElementById("status");

    if (notes.value === '') {
        notes.required = true;
    } else {
        status.value = 'Deny';
    }
}

function approveAppt() {
    const notes = document.getElementById("apptNotes");
    const status = document.getElementById("status");
    notes.required = false;
    status.value = 'Approve';
}