document.addEventListener("DOMContentLoaded", () => {
  // Mobile Sidebar Toggle
  const sidebar = document.getElementById("sidebar");
  const openSidebar = document.getElementById("openSidebar");
  const closeSidebar = document.getElementById("closeSidebar");

  if (openSidebar) {
    openSidebar.addEventListener("click", () => sidebar.classList.add("show"));
  }
  if (closeSidebar) {
    closeSidebar.addEventListener("click", () => sidebar.classList.remove("show"));
  }

  // Simulated Patient Database for Prototype Demo
  const samplePatients = [
    {
      id: "PT-2049",
      name: "Ahmed Farouk",
      meta: "Age: 34 | Blood Type: O+ | Cairo, Egypt",
      allergies: [
        "<strong>Penicillin:</strong> Severe skin rash and swelling.",
        "<strong>Peanuts:</strong> Mild respiratory sensitivity."
      ],
      chronic: [
        "<strong>Condition:</strong> Mild Hypertension (Diagnosed 2024)",
        "<strong>Current Medication:</strong> Concor 2.5mg (Once daily)"
      ]
    },
    {
      id: "PT-1892",
      name: "Nourhan Adel",
      meta: "Age: 28 | Blood Type: A+ | Giza, Egypt",
      allergies: ["<strong>Sulfa Drugs:</strong> Hives and itching."],
      chronic: ["<strong>Condition:</strong> Asthma (Mild, inhaler as needed)"]
    }
  ];

  let currentFoundPatient = null;

  const searchPatientForm = document.getElementById("searchPatientForm");
  const searchQuery = document.getElementById("searchQuery");
  const searchResultBox = document.getElementById("searchResultBox");
  const resPatientId = document.getElementById("resPatientId");
  const resPatientName = document.getElementById("resPatientName");
  const resPatientMeta = document.getElementById("resPatientMeta");
  const requestAccessBtn = document.getElementById("requestAccessBtn");
  const verifyOtpBtn = document.getElementById("verifyOtpBtn");
  const otpInput = document.getElementById("otpInput");
  const accessFeedback = document.getElementById("accessFeedback");
  const activeRecordsSection = document.getElementById("active-records");

  // Search Patient Handler
  if (searchPatientForm) {
    searchPatientForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const q = searchQuery.value.trim().toUpperCase().replace("#", "");
      const found = samplePatients.find(
        (p) => p.id.includes(q) || p.name.toUpperCase().includes(q)
      );

      searchResultBox.classList.remove("d-none");
      activeRecordsSection.classList.add("d-none");
      otpInput.value = "";

      if (found) {
        currentFoundPatient = found;
        resPatientId.textContent = `#${found.id}`;
        resPatientName.textContent = found.name;
        resPatientMeta.textContent = found.meta;
        document.getElementById("accessControls").classList.remove("d-none");
        accessFeedback.className = "mt-2 small fw-semibold text-muted";
        accessFeedback.textContent =
          "Patient found. Send an access request or enter the 6-digit OTP provided by the patient.";
      } else {
        currentFoundPatient = null;
        resPatientId.textContent = "Not Found";
        resPatientName.textContent = "No matching patient record";
        resPatientMeta.textContent = "Try searching for PT-2049 or PT-1892";
        document.getElementById("accessControls").classList.add("d-none");
        accessFeedback.textContent = "";
      }
    });
  }

  // Send Access Request Button
  if (requestAccessBtn) {
    requestAccessBtn.addEventListener("click", () => {
      if (!currentFoundPatient) return;
      accessFeedback.className = "mt-2 small fw-semibold text-primary";
      accessFeedback.innerHTML = `<i class="bi bi-check-circle me-1"></i> Access request sent to <strong>${currentFoundPatient.name}</strong>. Ask the patient for their 6-digit OTP once approved.`;
    });
  }

  // Verify OTP & Unlock Record
  if (verifyOtpBtn) {
    verifyOtpBtn.addEventListener("click", () => {
      if (!currentFoundPatient) return;
      const code = otpInput.value.trim();

      // Accept any 6-digit numeric code for frontend prototype testing
      if (/^\d{6}$/.test(code)) {
        accessFeedback.className = "mt-2 small fw-semibold text-success";
        accessFeedback.innerHTML = `<i class="bi bi-shield-check me-1"></i> OTP Verified! Temporary access granted.`;

        // Populate Unlocked Patient Record
        document.getElementById("unlockedName").textContent = `${currentFoundPatient.name} (#${currentFoundPatient.id})`;
        document.getElementById("docAllergiesList").innerHTML = currentFoundPatient.allergies
          .map((a) => `<li>${a}</li>`)
          .join("");
        document.getElementById("docChronicList").innerHTML = currentFoundPatient.chronic
          .map((c) => `<li>${c}</li>`)
          .join("");

        activeRecordsSection.classList.remove("d-none");
        activeRecordsSection.scrollIntoView({ behavior: "smooth" });
      } else {
        accessFeedback.className = "mt-2 small fw-semibold text-danger";
        accessFeedback.innerHTML = `<i class="bi bi-exclamation-circle me-1"></i> Invalid OTP. Please enter a valid 6-digit verification code.`;
      }
    });
  }

  // Add New Consultation Note
  const consultationForm = document.getElementById("consultationForm");
  if (consultationForm) {
    consultationForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const diagnosis = document.getElementById("consultDiagnosis").value.trim();
      const notes = document.getElementById("consultNotes").value.trim();
      const today = new Date().toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric"
      });

      if (diagnosis && notes && currentFoundPatient) {
        // Add to patient's unlocked table
        const newRow = `
          <tr class="table-success">
            <td>${today}</td>
            <td>Dr. Karim Nabil (Internal Medicine)</td>
            <td><strong>${diagnosis}</strong></td>
            <td>${notes}</td>
          </tr>
        `;
        document
          .getElementById("unlockedHistoryTable")
          .insertAdjacentHTML("afterbegin", newRow);

        // Add to doctor's recent activity log
        const activityRow = `
          <tr>
            <td>${today}</td>
            <td>#${currentFoundPatient.id}</td>
            <td>${currentFoundPatient.name}</td>
            <td>${diagnosis}</td>
          </tr>
        `;
        document
          .getElementById("doctorActivityTable")
          .insertAdjacentHTML("afterbegin", activityRow);

        consultationForm.reset();
        const modalEl = document.getElementById("newConsultationModal");
        const modalInstance = bootstrap.Modal.getInstance(modalEl);
        if (modalInstance) modalInstance.hide();
      }
    });
  }
});