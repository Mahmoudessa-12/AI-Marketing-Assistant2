const csvFile = document.getElementById("csvFile");
const dropZone = document.getElementById("dropZone");
const fileInfo = document.getElementById("fileInfo");

const generateBtn = document.getElementById("generateBtn");
const loading = document.getElementById("loading");
const errorMessage = document.getElementById("errorMessage");

const resultsSection = document.getElementById("resultsSection");

const campaignSummary = document.getElementById("campaignSummary");
const campaignTableBody = document.getElementById("campaignTableBody");
const personasContainer = document.getElementById("personasContainer");
const jsonViewer = document.getElementById("jsonViewer");

const copyJsonBtn = document.getElementById("copyJsonBtn");
const exportCsvBtn = document.getElementById("exportCsvBtn");
const exportPdfBtn = document.getElementById("exportPdfBtn");

let selectedFile = null;
let campaignData = null;


// ===============================
// CSV FILE SELECTION
// ===============================

csvFile.addEventListener("change", function () {

    if (this.files.length > 0) {

        selectedFile = this.files[0];

        showFileInfo(selectedFile);
    }
});


// ===============================
// DRAG & DROP
// ===============================

dropZone.addEventListener("dragover", function (event) {

    event.preventDefault();

    dropZone.classList.add("drag-over");
});


dropZone.addEventListener("dragleave", function () {

    dropZone.classList.remove("drag-over");
});


dropZone.addEventListener("drop", function (event) {

    event.preventDefault();

    dropZone.classList.remove("drag-over");

    const files = event.dataTransfer.files;

    if (files.length > 0) {

        const file = files[0];

        if (!file.name.toLowerCase().endsWith(".csv")) {

            showError("Please select a CSV file.");

            return;
        }

        selectedFile = file;

        showFileInfo(file);
    }
});


// ===============================
// SHOW FILE INFO
// ===============================

function showFileInfo(file) {

    fileInfo.innerHTML = `
        <i class="bi bi-check-circle"></i>
        ${file.name}
        (${formatFileSize(file.size)})
    `;

    hideError();
}


function formatFileSize(bytes) {

    if (bytes < 1024) {

        return bytes + " B";
    }

    if (bytes < 1024 * 1024) {

        return (bytes / 1024).toFixed(1) + " KB";
    }

    return (bytes / (1024 * 1024)).toFixed(1) + " MB";
}


// ===============================
// GENERATE CAMPAIGN
// ===============================

generateBtn.addEventListener("click", async function () {

    hideError();

    const campaignGoal =
        document.getElementById("campaignGoal").value.trim();

    const outputLanguage =
        document.querySelector(
            'input[name="outputLanguage"]:checked'
        ).value;

    const targetBudget =
        document.getElementById("targetBudget").value;

    const durationWeeks =
        document.getElementById("durationWeeks").value;


    // Validate CSV

    if (!selectedFile) {

        showError("Please select a CSV file.");

        return;
    }


    // Validate file type

    if (!selectedFile.name.toLowerCase().endsWith(".csv")) {

        showError("Only CSV files are allowed.");

        return;
    }


    // Validate campaign goal

    if (!campaignGoal) {

        showError("Please enter a campaign goal.");

        return;
    }


    // Validate budget

    if (!targetBudget || Number(targetBudget) <= 0) {

        showError("Please enter a valid budget.");

        return;
    }


    // Validate duration

    if (!durationWeeks || Number(durationWeeks) <= 0) {

        showError("Please enter a valid duration.");

        return;
    }


    // Create FormData

    const formData = new FormData();

    formData.append("File", selectedFile);

    formData.append("CampaignGoal", campaignGoal);

    // NEW: Output Language

    formData.append("OutputLanguage", outputLanguage);

    formData.append("TargetBudget", targetBudget);

    formData.append("DurationWeeks", durationWeeks);


    // Loading state

    setLoading(true);


    try {

        const response = await fetch(
            "/api/Campaign/generate",
            {
                method: "POST",
                body: formData
            }
        );


        const data = await response.json();


        if (!response.ok || !data.success) {

            throw new Error(
                data.message ||
                "Failed to generate campaign."
            );
        }


        // Parse AI result

        let result = data.result;

        if (typeof result === "string") {

            result = cleanJsonResponse(result);

            result = JSON.parse(result);
        }


        campaignData = result;


        // Display results

        displayCampaign(result);

        resultsSection.classList.remove("d-none");

        resultsSection.scrollIntoView({
            behavior: "smooth"
        });

    }
    catch (error) {

        console.error(error);

        showError(
            error.message ||
            "Something went wrong."
        );

    }
    finally {

        setLoading(false);
    }
});


// ===============================
// CLEAN AI JSON
// ===============================

function cleanJsonResponse(text) {

    let cleaned = text.trim();


    if (cleaned.startsWith("```json")) {

        cleaned = cleaned
            .replace(/^```json\s*/i, "")
            .replace(/\s*```$/i, "");
    }

    else if (cleaned.startsWith("```")) {

        cleaned = cleaned
            .replace(/^```\s*/i, "")
            .replace(/\s*```$/i, "");
    }


    return cleaned.trim();
}


// ===============================
// DISPLAY CAMPAIGN
// ===============================

function displayCampaign(data) {

    const campaign =
        data.campaign || data;


    // Summary

    campaignSummary.textContent =
        campaign.summary ||
        "Campaign generated successfully.";


    // Calendar

    campaignTableBody.innerHTML = "";


    const calendar =
        campaign.calendar || [];


    calendar.forEach(item => {

        const row =
            document.createElement("tr");


        row.innerHTML = `
            <td>${escapeHtml(item.week)}</td>

            <td>
                <span class="badge bg-primary">
                    ${escapeHtml(item.channel)}
                </span>
            </td>

            <td>${escapeHtml(item.objective)}</td>

            <td>${escapeHtml(item.audience)}</td>

            <td>${escapeHtml(item.contentType)}</td>

            <td>${escapeHtml(item.copy)}</td>

            <td>${escapeHtml(item.budget)}</td>

            <td>${escapeHtml(item.kpi)}</td>
        `;


        campaignTableBody.appendChild(row);
    });


    // Personas

    personasContainer.innerHTML = "";


    const personas =
        campaign.personas || [];


    personas.forEach(persona => {

        const painPoints =
            persona.painPoints || [];


        const copyVariations =
            persona.copyVariations || [];


        const card =
            document.createElement("div");

        card.className = "col-md-6";


        card.innerHTML = `
            <div class="persona-card">

                <h5>
                    <i class="bi bi-person-circle"></i>
                    ${escapeHtml(persona.name)}
                </h5>

                <p class="text-secondary">
                    ${escapeHtml(persona.description)}
                </p>

                <h6 class="mt-4">
                    Pain Points
                </h6>

                <ul>
                    ${painPoints.map(point =>
            `<li>${escapeHtml(point)}</li>`
        ).join("")}
                </ul>

                <h6 class="mt-4">
                    Copy Variations
                </h6>

                ${copyVariations.map((copy, index) => `
                    <div class="alert alert-secondary">
                        <strong>Variation ${index + 1}</strong>
                        <br>
                        ${escapeHtml(copy)}
                    </div>
                `).join("")}

            </div>
        `;


        personasContainer.appendChild(card);
    });


    // JSON Viewer

    jsonViewer.textContent =
        JSON.stringify(data, null, 2);
}


// ===============================
// COPY JSON
// ===============================

copyJsonBtn.addEventListener("click", async function () {

    if (!campaignData) {

        return;
    }


    const json =
        JSON.stringify(
            campaignData,
            null,
            2
        );


    try {

        await navigator.clipboard.writeText(json);

        copyJsonBtn.innerHTML =
            '<i class="bi bi-check"></i> Copied!';


        setTimeout(() => {

            copyJsonBtn.innerHTML =
                '<i class="bi bi-clipboard"></i> Copy Code';

        }, 2000);

    }
    catch (error) {

        console.error(error);
    }
});


// ===============================
// EXPORT CSV
// ===============================

exportCsvBtn.addEventListener("click", function () {

    if (!campaignData) {

        return;
    }


    const campaign =
        campaignData.campaign ||
        campaignData;


    const calendar =
        campaign.calendar || [];


    if (calendar.length === 0) {

        alert("No campaign calendar available.");

        return;
    }


    const headers = [
        "Week",
        "Channel",
        "Objective",
        "Audience",
        "Content Type",
        "Copy",
        "Budget",
        "KPI"
    ];


    const rows = calendar.map(item => [

        item.week,
        item.channel,
        item.objective,
        item.audience,
        item.contentType,
        item.copy,
        item.budget,
        item.kpi

    ]);


    const csv = [
        headers,
        ...rows
    ]
        .map(row =>
            row.map(value =>
                `"${String(value ?? "")
                    .replace(/"/g, '""')}"`
            ).join(",")
        )
        .join("\n");


    const blob =
        new Blob(
            [csv],
            {
                type: "text/csv;charset=utf-8;"
            }
        );


    const url =
        URL.createObjectURL(blob);


    const link =
        document.createElement("a");


    link.href = url;

    link.download =
        "marketing-campaign.csv";


    link.click();


    URL.revokeObjectURL(url);
});


// ===============================
// EXPORT PDF
// ===============================

exportPdfBtn.addEventListener("click", function () {

    if (!campaignData) {

        return;
    }


    const {
        jsPDF
    } = window.jspdf;


    const pdf =
        new jsPDF();


    const campaign =
        campaignData.campaign ||
        campaignData;


    let y = 20;


    pdf.setFontSize(20);

    pdf.text(
        "AI Marketing Campaign",
        20,
        y
    );


    y += 15;


    pdf.setFontSize(11);


    pdf.text(
        "Summary:",
        20,
        y
    );


    y += 8;


    const summary =
        campaign.summary ||
        "";


    const summaryLines =
        pdf.splitTextToSize(
            summary,
            170
        );


    pdf.text(
        summaryLines,
        20,
        y
    );


    y +=
        summaryLines.length * 6 +
        10;


    pdf.text(
        "Campaign Calendar:",
        20,
        y
    );


    y += 8;


    const calendar =
        campaign.calendar || [];


    calendar.forEach(item => {

        const text =
            `Week ${item.week} - ` +
            `${item.channel} - ` +
            `${item.objective}`;


        const lines =
            pdf.splitTextToSize(
                text,
                170
            );


        if (y > 270) {

            pdf.addPage();

            y = 20;
        }


        pdf.text(
            lines,
            20,
            y
        );


        y +=
            lines.length * 6 +
            5;
    });


    pdf.save(
        "marketing-campaign.pdf"
    );
});


// ===============================
// UI HELPERS
// ===============================

function setLoading(isLoading) {

    if (isLoading) {

        generateBtn.disabled = true;

        generateBtn.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2"></span>' +
            'Generating...';

        loading.classList.remove("d-none");

    }
    else {

        generateBtn.disabled = false;

        generateBtn.innerHTML =
            '<i class="bi bi-stars"></i> Generate Campaign';

        loading.classList.add("d-none");
    }
}


function showError(message) {

    errorMessage.textContent = message;

    errorMessage.classList.remove("d-none");
}


function hideError() {

    errorMessage.classList.add("d-none");

    errorMessage.textContent = "";
}


// ===============================
// HTML ESCAPE
// ===============================

function escapeHtml(value) {

    if (value === null ||
        value === undefined) {

        return "";
    }


    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}