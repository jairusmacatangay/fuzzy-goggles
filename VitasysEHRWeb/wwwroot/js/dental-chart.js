let loadToothForm = (toothNo, type, id, editable) => {
    if (editable === "yes") {
        const url = `/PatientRecords/DentalChart/LoadToothForm?toothNo=${toothNo}&type=${type}&id=${id}`;

        $("#toothFromModalBodyDiv").load(url, function () {
            document.getElementById("toothFromModalLabel").innerHTML = `Tooth No. ${toothNo}`;
            $("#toothFromModal").modal("show");

            const conditionSelect = document.getElementById("conditionSelect");
            const toothLabelPartSelect = document.getElementById("toothLabelPartSelect");

            conditionSelect.onchange = () => {
                if (conditionSelect.value == 1)
                    toothLabelPartSelect.style.display = "block";
                else
                    toothLabelPartSelect.style.display = "none";
            };
        });
    }
};