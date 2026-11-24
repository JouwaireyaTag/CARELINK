# CARELINK – Requirements Document

## 1. Project Overview

CARELINK is a web-based medical history management system designed to help patients securely store and manage their medical information and allow authorized doctors to access relevant patient records with the patient's permission.

## 2. Project Objectives

- Provide patients with a centralized platform to manage their medical history.
- Allow doctors to access patient medical records securely after receiving permission.
- Improve the availability and organization of medical information.
- Protect patient privacy and ensure secure access to sensitive medical data.

## 3. System Actors

### 3.1 Patient
The patient can:
- Register and log in.
- View and update their profile.
- Manage medical history.
- Approve or reject doctor access requests.
- Revoke previously granted access.

### 3.2 Doctor
The doctor can:
- Register and log in.
- Search for patients.
- Request access to patient medical records.
- Enter a verification code after patient approval.
- View authorized medical history.

### 3.3 Admin
The admin can:
- Log in.
- Approve or reject doctor registrations.
- Manage users and roles.
- View system audit logs.

## 4. Functional Requirements

- FR-01: The system shall allow patients to register and log in.
- FR-02: The system shall allow doctors to register and log in.
- FR-03: The system shall allow administrators to manage users and roles.
- FR-04: The system shall allow administrators to verify doctor accounts.
- FR-05: The system shall allow patients to create and update their profiles.
- FR-06: The system shall allow patients to manage their medical history, including conditions, allergies, surgeries, and medications.
- FR-07: The system shall allow doctors to search for patients.
- FR-08: The system shall allow doctors to request access to patient medical records.
- FR-09: The system shall allow patients to approve or reject access requests.
- FR-10: The system shall generate a verification code for approved access requests.
- FR-11: The system shall validate verification codes before granting access.
- FR-12: The system shall allow authorized doctors to view permitted medical records.
- FR-13: The system shall record important system activities in audit logs.

## 5. Non-Functional Requirements

- Security: The system shall protect user accounts and medical information.
- Privacy: Patient records shall only be accessible to authorized users.
- Usability: The system shall provide a simple and user-friendly interface.
- Reliability: The system shall maintain accurate and consistent medical records.
- Performance: The system shall respond to user requests within a reasonable time.
- Validation: The system shall validate user input and prevent invalid data.

## 6. Technology Stack

- Backend: ASP.NET Core MVC and C#
- Database: SQL Server
- ORM: Entity Framework Core
- Frontend: HTML, CSS, JavaScript, and Bootstrap
- Version Control: Git and GitHub

## 7. Project Scope

The project focuses on secure patient medical history management, doctor verification, and permission-based access to patient records.

Advanced features such as AI-based diagnosis and external hospital system integration are outside the initial project scope.

## 8. Security Considerations

- Role-based authorization shall be applied.
- Patient approval shall be required before doctors access medical records.
- Verification codes shall be validated before access is granted.
- Sensitive information shall not be exposed to unauthorized users.
- Only fictional or anonymized data shall be used for testing and demonstration.
