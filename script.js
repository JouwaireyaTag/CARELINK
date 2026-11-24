document.addEventListener("DOMContentLoaded", () => {
  const authCard = document.getElementById("authCard");
  const goRegisterBtn = document.getElementById("goRegisterBtn");
  const goLoginBtn = document.getElementById("goLoginBtn");
  const loginForm = document.getElementById("loginForm");
  const registerForm = document.getElementById("registerForm");
  const togglePassBtns = document.querySelectorAll(".toggle-pass");
  const forgotPasswordLink = document.getElementById("forgotPasswordLink");

  // Role selectors
  const loginRole = document.getElementById("loginRole");
  const loginUserLabel = document.getElementById("loginUserLabel");
  const loginUserInput = document.getElementById("loginUser");
  const regRole = document.getElementById("regRole");
  const licenseGroup = document.getElementById("licenseGroup");

  // Switch between Login and Register modes
  function setMode(mode) {
    if (!authCard) return;
    if (mode === "register") {
      authCard.classList.add("is-register");
    } else {
      authCard.classList.remove("is-register");
    }
  }

  setMode("login");

  if (goRegisterBtn) {
    goRegisterBtn.addEventListener("click", (e) => {
      e.preventDefault();
      setMode("register");
    });
  }

  if (goLoginBtn) {
    goLoginBtn.addEventListener("click", (e) => {
      e.preventDefault();
      setMode("login");
    });
  }

  // Update Login label/placeholder based on Role
  if (loginRole) {
    loginRole.addEventListener("change", (e) => {
      if (e.target.value === "doctor") {
        loginUserLabel.textContent = "Doctor ID or Email";
        loginUserInput.placeholder = "Enter doctor ID or email";
      } else {
        loginUserLabel.textContent = "Patient ID or Email";
        loginUserInput.placeholder = "Enter patient ID or email";
      }
    });
  }

  // Show/Hide Medical License field in Registration based on Role
  if (regRole) {
    regRole.addEventListener("change", (e) => {
      if (e.target.value === "doctor") {
        licenseGroup.style.display = "block";
      } else {
        licenseGroup.style.display = "none";
        setError("licenseError", "");
      }
    });
  }

  // Toggle Password Visibility with Bootstrap Icons
  togglePassBtns.forEach((btn) => {
    btn.addEventListener("click", () => {
      const targetId = btn.getAttribute("data-target");
      const input = document.getElementById(targetId);
      const icon = btn.querySelector("i");
      if (!input || !icon) return;

      const isPass = input.type === "password";
      input.type = isPass ? "text" : "password";
      icon.className = isPass ? "bi bi-eye-slash" : "bi bi-eye";
    });
  });

  // Helper to display validation errors
  function setError(id, msg) {
    const el = document.getElementById(id);
    if (el) el.textContent = msg;
  }

  // Login Form Validation
  if (loginForm) {
    loginForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const role = loginRole.value;
      const u = loginUserInput.value.trim();
      const p = document.getElementById("loginPassword").value;
      let ok = true;

      setError("loginUserError", "");
      setError("loginPasswordError", "");

      if (!u) {
        setError("loginUserError", `Please enter ${role} ID or email.`);
        ok = false;
      }
      if (!p) {
        setError("loginPasswordError", "Please enter your password.");
        ok = false;
      }

     if (ok) {
        if (u.toLowerCase().includes("admin")) {
          window.location.href = "admin-dashboard.html";
        } else if (role === "doctor") {
          window.location.href = "doctor-dashboard.html";
        } else {
          window.location.href = "patient-dashboard.html";
        }
      }
    });
  }

  // Register Form Validation
  if (registerForm) {
    registerForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const role = regRole.value;
      const n = document.getElementById("fullName").value.trim();
      const em = document.getElementById("email").value.trim();
      const lic = document.getElementById("medicalLicense").value.trim();
      const pw = document.getElementById("regPassword").value;
      let ok = true;

      setError("fullNameError", "");
      setError("emailError", "");
      setError("licenseError", "");
      setError("regPasswordError", "");

      if (n.length < 2) {
        setError("fullNameError", "Please enter your full name.");
        ok = false;
      }
      if (!em || !em.includes("@") || !em.includes(".")) {
        setError("emailError", "Please enter a valid email address.");
        ok = false;
      }
      if (role === "doctor" && lic.length < 4) {
        setError("licenseError", "Please enter a valid medical license number.");
        ok = false;
      }
      if (pw.length < 8) {
        setError("regPasswordError", "Password must be at least 8 characters.");
        ok = false;
      }

      if (ok) {
        if (role === "doctor") {
          alert(`Doctor account request submitted for Dr. ${n}. Pending admin approval.`);
        } else {
          alert(`Patient profile created for ${n}!`);
        }
      }
    });
  }

  // Forgot Password Link
  if (forgotPasswordLink) {
    forgotPasswordLink.addEventListener("click", (e) => {
      e.preventDefault();
      alert("Password reset instructions have been sent to your registered email.");
    });
  }
});