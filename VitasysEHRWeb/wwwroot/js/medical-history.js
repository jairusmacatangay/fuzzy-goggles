let loadCreateForm = (id) => {
    let url = `/PatientRecords/MedicalHistory/LoadCreateForm?id=${id}`
    $("#createMedHisModalBodyDiv").load(url, () => {
        $("#createMedHisModal").modal("show")
    })
}

let loadEditForm = (id) => {
    let url = `/PatientRecords/MedicalHistory/LoadEditRequestForm?id=${id}`
    $("#editMedHisModalBodyDiv").load(url, () => {
        $("#editMedHisModal").modal("show")
    })
}