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

  // Sample Doctor Access Requests Data
  let accessRequests = [
    {
      id: 1,
      doctor: "Dr. Karim Nabil",
      specialty: "Internal Medicine",
      clinic: "Al-Salam International Hospital",
      status: "Pending",
      otp: null,
      expires: null
    },
    {
      id: 2,
      doctor: "Dr. Mona El-Sayed",
      specialty: "Orthopedics",
      clinic: "Cairo Bone Clinic",
      status: "Pending",
      otp: null,
      expires: null
    }
  ];

  // Sample Medical History Data
  let medicalRecords = [
    { category: "Allergy", title: "Penicillin", notes: "Severe skin rash and swelling." },
    { category: "Allergy", title: "Peanuts", notes: "Mild respiratory sensitivity." },
    { category: "Chronic Condition", title: "Mild Hypertension", notes: "Diagnosed in 2024, controlled with diet." },
    { category: "Previous Surgery", title: "Appendectomy", notes: "Completed in 2019 without complications." },
    { category: "Medication", title: "Concor 2.5mg", notes: "Once daily in the morning." }
  ];

  const requestsTableBody = document.getElementById("requestsTableBody");
  const pendingCountBadge = document.getElementById("pendingCountBadge");
  const medicalHistoryGrid = document.getElementById("medicalHistoryGrid");

  // Generate 6-digit OTP
  function generateOTP() {
    return Math.floor(100000 + Math.random() * 900000).toString();
  }

  // Render Doctor Requests Table
  function renderRequests() {
    requestsTableBody.innerHTML = "";
    let pendingCount = 0;

    accessRequests.forEach((req) => {
      if (req.status === "Pending") pendingCount++;

      let statusBadge = "";
      if (req.status === "Pending") {
        statusBadge = `<span class="badge bg-warning text-dark">Pending</span>`;
      } else if (req.status === "Approved") {
        statusBadge = `<span class="badge bg-success">Approved (30m)</span>`;
      } else {
        statusBadge = `<span class="badge bg-danger">Rejected</span>`;
      }

      const otpDisplay = req.otp
        ? `<span class="otp-badge">${req.otp}</span>`
        : `<span class="text-muted small">—</span>`;

      const actionButtons =
        req.status === "Pending"
          ? `
            <button class="btn btn-sm btn-success me-1" onclick="handleRequest(${req.id}, 'Approved')">
              <i class="bi bi-check-lg"></i> Approve
            </button>
            <button class="btn btn-sm btn-outline-danger" onclick="handleRequest(${req.id}, 'Rejected')">
              <i class="bi bi-x-lg"></i> Reject
            </button>
          `
          : `<span class="text-muted small">Processed</span>`;

      const row = `
        <tr>
          <td class="fw-semibold">${req.doctor}</td>
          <td>${req.specialty}</td>
          <td>${req.clinic}</td>
          <td>${statusBadge}</td>
          <td>${otpDisplay}</td>
          <td class="text-end">${actionButtons}</td>
        </tr>
      `;
      requestsTableBody.insertAdjacentHTML("beforeend", row);
    });

    pendingCountBadge.textContent = pendingCount;
  }

  // Global handler for Approve / Reject buttons
  window.handleRequest = function (id, decision) {
    const req = accessRequests.find((r) => r.id === id);
    if (!req) return;

    req.status = decision;
    if (decision === "Approved") {
      req.otp = generateOTP();
    }
    renderRequests();
  };

  // Render Medical History Cards
  function renderMedicalHistory() {
    medicalHistoryGrid.innerHTML = "";
    medicalRecords.forEach((item) => {
      const card = `
        <div class="col-md-6 col-lg-4">
          <div class="history-item-card">
            <span class="category-tag">${item.category}</span>
            <h6 class="fw-bold mt-1 mb-2 text-dark">${item.title}</h6>
            <p class="text-muted small mb-0">${item.notes}</p>
          </div>
        </div>
      `;
      medicalHistoryGrid.insertAdjacentHTML("beforeend", card);
    });
  }

  // Add New Medical Record Form
  const addRecordForm = document.getElementById("addRecordForm");
  if (addRecordForm) {
    addRecordForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const category = document.getElementById("recordCategory").value;
      const title = document.getElementById("recordTitle").value.trim();
      const notes = document.getElementById("recordNotes").value.trim();

      if (title && notes) {
        medicalRecords.unshift({ category, title, notes });
        renderMedicalHistory();
        addRecordForm.reset();

        // Close Bootstrap Modal
        const modalEl = document.getElementById("addRecordModal");
        const modalInstance = bootstrap.Modal.getInstance(modalEl);
        if (modalInstance) modalInstance.hide();
      }
    });
  }

  // Initial render
  renderRequests();
  renderMedicalHistory();
});