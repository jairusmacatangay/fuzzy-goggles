let editClinicLogo = () => {
    let url = `/Clinic/Account/LoadEditLogo`

    $("#editClinicLogoModalBodyDiv").load(url, function () {
        $("#editClinicLogoModal").modal("show");
    })
}

let editClinicAccount = () => {
    let url = `/Clinic/Account/LoadEditForm`

    $("#editClinicAccountModalBodyDiv").load(url, function () {
        $("#editClinicAccountModal").modal("show");
    })
}

let viewPayments = () => {
    let url = `/Clinic/Subscription/LoadPaymentsList`

    const viewPaymentsModalBodyDiv = document.getElementById("viewPaymentsModalBodyDiv");
  
    if (viewPaymentsModalBodyDiv.innerHTML.trim() == "") {
        $("#viewPaymentsModalBodyDiv").load(url, function () {
            $("#viewPaymentsModal").modal("show");
        });
    } else {
        $("#viewPaymentsModal").modal("show");
    }
}