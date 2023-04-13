const selectedReport = document.getElementById("selectedReport");
const generateReportBtn = document.getElementById("generateReportBtn");
const generatedReport = document.getElementById("generatedReport");
const selectedReportError = document.getElementById("selectedReportError");
const reportForm = document.getElementById("reportForm");

let report;

selectedReport.onchange = () => {
    report = selectedReport.value;

    if (report == "5") {
        loadForm();
    } else {
        clearForm();
    }
}

generateReportBtn.onclick = () => {
    report = selectedReport.value;

    if (selectedReportError.innerHTML.trim() != "")
        selectedReportError.innerHTML = "";

    if (report == "1")
        loadReport("/Clinic/Report/LoadPatientReport");
    else if (report == "2")
        loadReport("/Clinic/Report/LoadTreatmentReport");
    else if (report == "3")
        loadReport("/Clinic/Report/LoadInvoiceReport");
    else if (report == "4")
        loadReport("/Clinic/Report/LoadAuditLogsReport");
    else if (report == "5")
        loadDentalSummary();
    else
        selectedReportError.innerHTML = "This field is required.";
};

let clearReport = () => generatedReport.innerHTML = "";

let clearForm = () => reportForm.innerHTML = "";

let loadReport = (url) => {
    clearReport();
    $("#generatedReport").load(url, (response, status, xhr) => {
        if (status == "error") {
            toastr.options = { "positionClass": "toast-bottom-center" };
            toastr.error("Ooops! Something went wrong while generating your report.");
        }
    });
};

let loadForm = () => {
    $("#reportForm").load("/Clinic/Report/LoadDentalSummaryForm", (response, status, xhr) => {
        if (status == "error") {
            toastr.options = { "positionClass": "toast-bottom-center" };
            toastr.error("Ooops! Something went wrong while generating your report.");
        }
    });
};

let loadDentalSummary = () => {
    let patientId = document.getElementById("patientId").value;
    let patientName = document.getElementById("patientName").value;
    let patientInputError = document.getElementById("patientInputError");

    if (patientName == "") {
        patientInputError.innerHTML = "Patient Name is required.";
    } else if (patientName != "" && patientId == "") {
        patientInputError.innerHTML = "Patient Name is required. Please select from the autocomplete options.";
    } else {
        if (patientInputError.innerHTML.trim() != "")
            patientInputError.innerHTML = "";

        loadReport(`/Clinic/Report/LoadDentalSummaryReport?patientId=${parseInt(patientId)}&isPrint=false&clinicId=0`);
    }      
};