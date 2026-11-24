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

  // Sample Pending Doctors (Including Dr. Fatma Ahmed from your test!)
  let pendingDoctors = [
    {
      id: 1,
      name: "Dr. Fatma Ahmed",
      email: "fa3582276@gmail.com",
      license: "01146700514",
      date: "24 Nov 2026",
      status: "Pending"
    },
    {
      id: 2,
      name: "Dr. Mostafa Nour Eldin",
      email: "m.noureldin@hospital.eg",
      license: "MED-994821",
      date: "23 Nov 2026",
      status: "Pending"
    }
  ];

  // Sample Audit Logs Data
  const auditLogs = [
    {
      time: "24 Nov 2026, 03:45 AM",
      actor: "Dr. Karim Nabil (#DR-104)",
      patient: "Ahmed Farouk (#PT-2049)",
      action: "Viewed Medical History & Added Consultation",
      method: "OTP Verified (30m Session)"
    },
    {
      time: "23 Nov 2026, 08:12 PM",
      actor: "Ahmed Farouk (#PT-2049)",
      patient: "Self (#PT-2049)",
      action: "Approved Access Request for Dr. Karim Nabil",
      method: "Patient Consent Portal"
    },
    {
      time: "23 Nov 2026, 05:30 PM",
      actor: "Dr. Sara Mahmoud (#DR-088)",
      patient: "Nourhan Adel (#PT-1892)",
      action: "Updated Prescription Notes",
      method: "OTP Verified (30m Session)"
    },
    {
      time: "22 Nov 2026, 11:15 AM",
      actor: "Dr. Tarek Hassan (#DR-052)",
      patient: "Mahmoud Samir (#PT-1405)",
      action: "Requested Medical Record Access",
      method: "Access Request Sent"
    }
  ];

  let verifiedCount = 86;

  const doctorApprovalsBody = document.getElementById("doctorApprovalsBody");
  const pendingDoctorsBadge = document.getElementById("pendingDoctorsBadge");
  const pendingStatCount = document.getElementById("pendingStatCount");
  const verifiedDoctorsCount = document.getElementById("verifiedDoctorsCount");
  const auditLogsBody = document.getElementById("auditLogsBody");
  const logFilterInput = document.getElementById("logFilterInput");

  // Render Pending Doctors Table
  function renderDoctors() {
    doctorApprovalsBody.innerHTML = "";
    let pendingCount = 0;

    pendingDoctors.forEach((doc) => {
      if (doc.status === "Pending") pendingCount++;

      let statusBadge = "";
      if (doc.status === "Pending") {
        statusBadge = `<span class="badge bg-warning text-dark">Pending Review</span>`;
      } else if (doc.status === "Verified") {
        statusBadge = `<span class="badge bg-success">Verified</span>`;
      } else {
        statusBadge = `<span class="badge bg-danger">Rejected</span>`;
      }

      const actions =
        doc.status === "Pending"
          ? `
            <button class="btn btn-sm btn-success me-1" onclick="verifyDoctor(${doc.id}, 'Verified')">
              <i class="bi bi-check-circle"></i> Approve
            </button>
            <button class="btn btn-sm btn-outline-danger" onclick="verifyDoctor(${doc.id}, 'Rejected')">
              <i class="bi bi-x-circle"></i> Reject
            </button>
          `
          : `<span class="text-muted small">Completed</span>`;

      const row = `
        <tr>
          <td class="fw-semibold">${doc.name}</td>
          <td>${doc.email}</td>
          <td><code>${doc.license}</code></td>
          <td>${doc.date}</td>
          <td>${statusBadge}</td>
          <td class="text-end">${actions}</td>
        </tr>
      `;
      doctorApprovalsBody.insertAdjacentHTML("beforeend", row);
    });

    pendingDoctorsBadge.textContent = pendingCount;
    pendingStatCount.textContent = pendingCount;
    verifiedDoctorsCount.textContent = verifiedCount;
  }

  // Global handler for Doctor Approval
  window.verifyDoctor = function (id, decision) {
    const doc = pendingDoctors.find((d) => d.id === id);
    if (!doc) return;

    doc.status = decision;
    if (decision === "Verified") {
      verifiedCount++;
      auditLogs.unshift({
        time: "Just now",
        actor: "Admin (Jowaireya Ahmed)",
        patient: "N/A",
        action: `Verified Physician Account: ${doc.name}`,
        method: "Admin License Check"
      });
      renderAuditLogs(logFilterInput.value);
    }
    renderDoctors();
  };

  // Render Audit Logs Table
  function renderAuditLogs(filter = "") {
    auditLogsBody.innerHTML = "";
    const q = filter.trim().toLowerCase();

    const filtered = auditLogs.filter(
      (log) =>
        log.actor.toLowerCase().includes(q) ||
        log.patient.toLowerCase().includes(q) ||
        log.action.toLowerCase().includes(q)
    );

    filtered.forEach((log) => {
      const row = `
        <tr>
          <td class="small text-muted">${log.time}</td>
          <td class="fw-semibold">${log.actor}</td>
          <td>${log.patient}</td>
          <td>${log.action}</td>
          <td><span class="badge bg-light text-dark border">${log.method}</span></td>
        </tr>
      `;
      auditLogsBody.insertAdjacentHTML("beforeend", row);
    });
  }

  if (logFilterInput) {
    logFilterInput.addEventListener("input", (e) => {
      renderAuditLogs(e.target.value);
    });
  }

  renderDoctors();
  renderAuditLogs();
});