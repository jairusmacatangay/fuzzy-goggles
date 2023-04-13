let loadAddTreatmentForm = (id) => {
    const url = `/Clinic/Invoice/LoadAddTreatmentForm?id=${id}`;
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
    $.post("/Clinic/Invoice/IncrementQuantity", { id: id }, () => {
        location.reload()
    });
}

let decrementQuantity = (id) => {
    $.post("/Clinic/Invoice/DecrementQuantity", { id: id }, () => {
        location.reload()
    });
}

let loadAddToothForm = (id) => {
    const url = `/Clinic/Invoice/LoadAddToothForm?id=${id}`;
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
    const url = `/Clinic/Invoice/LoadAddDentistForm?id=${id}`;
    const addDentistModalBody = document.getElementById("addDentistModalBody");

    if (addDentistModalBody.innerHTML.trim() == "") {
        $("#addDentistModalBody").load(url, function () {
            $("#addDentistModal").modal("show");
        });
    } else {
        $("#addDentistModal").modal("show");
    }
};