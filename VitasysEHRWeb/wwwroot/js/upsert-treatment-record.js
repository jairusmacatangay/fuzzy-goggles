let loadAddTreatmentForm = (id) => {
    const url = `/PatientRecords/Treatment/LoadAddTreatmentForm?id=${id}`;
    const createTreatmentModalBody = document.getElementById("createTreatmentModalBody");

    if (createTreatmentModalBody.innerHTML.trim() == "") {
        $("#createTreatmentModalBody").load(url, function () {
            $("#createTreatmentModal").modal("show");
        });
    } else {
        $("#createTreatmentModal").modal("show");
    }
};

let incrementQuantity = (id) => {
    $.post("/PatientRecords/Treatment/IncrementQuantity", { id: id }, () => {
        location.reload()
    });
}

let decrementQuantity = (id) => {
    $.post("/PatientRecords/Treatment/DecrementQuantity", { id: id }, () => {
        location.reload()
    });
}

let loadAddToothForm = (id) => {
    const url = `/PatientRecords/Treatment/LoadAddToothForm?id=${id}`;
    const addToothModalBody = document.getElementById("addToothModalBody");

    if (addToothModalBody.innerHTML.trim() == "") {
        $("#addToothModalBody").load(url, function () {
            $("#addToothModal").modal("show");
        });
    } else {
        $("#addToothModal").modal("show");
    }
};

let loadAddDentistForm = (id) => {
    const url = `/PatientRecords/Treatment/LoadAddDentistForm?id=${id}`;
    const addDentistModalBody = document.getElementById("addDentistModalBody");

    if (addDentistModalBody.innerHTML.trim() == "") {
        $("#addDentistModalBody").load(url, function () {
            $("#addDentistModal").modal("show");
        });
    } else {
        $("#addDentistModal").modal("show");
    }
};