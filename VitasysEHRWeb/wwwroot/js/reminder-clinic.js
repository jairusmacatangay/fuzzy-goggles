let dataTable;

$(document).ready(function () {
    let id = document.getElementById("patientId").value;

    loadDataTable(id);
});

let loadDataTable = (id) => {
    dataTable = $('#remindersTable').DataTable({
        "ajax": {
            "url": `/PatientRecords/Reminder/GetAllReminders?id=${id}`,
            "dataSrc": ""
        },
        "oLanguage": {
            "sSearch": '<i class="fa fa-search me-1"></i>Search:',
        },
        "columns": [
            { "data": "Id" },
            { "data": "Title" },
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
                    return `
                        <div class="dropdown">
                            <button class="btn btn-outline-primary rounded-3 dropdown-toggle" type="button" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-hand-pointer me-2"></i>Action
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item text-muted pointer" onclick="loadReminder(${data})"><div class="row"><div class="col-2"><i class="fas fa-search-plus me-2"></i></div><div class="col-10">View</div></div></a></li>
                                <li><a href="/PatientRecords/Reminder/Update/${data}" class="dropdown-item text-muted"><div class="row"><div class="col-2"><i class="fas fa-edit me-2"></i></div><div class="col-10">Update</div></div></a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li><a class="dropdown-item text-muted pointer" onclick="deleteReminder('/PatientRecords/Reminder/Delete/${data}')"><div class="row"><div class="col-2"><i class="fas fa-trash"></i></div><div class="col-10">Delete</div></div></a></li>
                            </ul>
                        </div>`
                }
            }
        ]
    });
};

async function loadReminder(id) {
    let response = await fetch(`/PatientRecords/Reminder/LoadReminder/${id}`);

    if (response.ok) {
        document.getElementById("reminderModalBodyDiv").innerHTML = await response.text();

        var reminderModal = new bootstrap.Modal(document.getElementById('reminderModal'));
        reminderModal.show();
    } else {
        toastr.options = { "positionClass": "toast-top-center" }
        toastr.error("Reminder does not exist.");
    }
}

let deleteReminder = (url) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "This record will be permanently deleted!",
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
            });
        }
    })
}