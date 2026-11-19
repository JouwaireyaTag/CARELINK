# CARELINK
### One Patient. One Trusted Medical History.

## Project Idea

CARELINK is a web-based medical history management system that provides authorized doctors with organized access to patients' medical histories.

The system aims to help doctors review relevant patient information, including previous surgeries, allergies, chronic conditions, and medications, through a centralized digital platform.

The project is developed as part of the Digital Egypt Pioneers Initiative (DEPI) Full Stack .NET Web Development track.

## Problem Statement

Patients may forget important details about their medical history, including previous surgeries, allergies, chronic conditions, and medications. Their medical records may also be scattered across different clinics or paper documents.

CARELINK aims to organize medical history in one centralized system, helping authorized doctors review available patient information more efficiently during consultations.

## Project Objectives

- Organize patients' medical history in a structured digital format.
- Help authorized doctors retrieve relevant medical records.
- Improve access to documented medical information during consultations.
- Apply authentication and role-based access control.
- Allow patients to control access to their medical records.
- Protect sensitive information and maintain accountability for record access and updates.

## Planned Features

- User authentication and authorization.
- Patient profile management.
- Medical history management.
- Recording previous surgeries, allergies, chronic conditions, and medications.
- Patient search and medical history retrieval.
- Doctor access requests to patient medical records.
- Patient approval or rejection of access requests.
- One-time verification codes for medical record access.
- Temporary access permissions with expiration.
- Consultation and visit records.
- Doctor verification and approval by the administrator.
- Administrative dashboard.
- Audit logs for medical record access and modifications.
- Input validation and access control.

*Note: Features are planned and may be refined during development.*

## Technology Stack

- **Backend:** C# and ASP.NET Core MVC
- **Architecture:** Model-View-Controller (MVC)
- **Frontend:** HTML, CSS, Bootstrap
- **Database:** Microsoft SQL Server
- **Development Environment:** Visual Studio
- **Version Control:** Git and GitHub

## Team Members

| Name | Role |
|------|------|
| Jowaireya Ahmed | Team Leader & System Analyst |
| Aya Medhat | Database Developer |
| Mostafa Nour Eldin | Backend Developer – Authentication & Security |
| Mourad Fikry | Backend Developer – Medical System |
| Fatma Ahmed | Frontend Developer |

*Team roles may be adjusted according to project requirements and team progress.*

## Work Plan

| Phase | Activities | Deliverables |
|-------|------------|--------------|
| Phase 1 | Project Analysis | Requirements Document, Use Case Diagram, Actors, User Flows |
| Phase 2 | Database Design | ERD, Database Entities, Relationships |
| Phase 3 | GitHub & Project Setup | Repository, Branches, ASP.NET Core MVC Project |
| Phase 4 | Database & Backend Development | Entities, Migrations, Authentication, Business Logic |
| Phase 5 | Frontend Development | UI Pages, Dashboards, Responsive Layouts |
| Phase 6 | Integration & Testing | Integrated System, Testing, Bug Fixes |
| Phase 7 | Finalization | Documentation, Setup Instructions, Final Presentation |

## Roles & Responsibilities

- **Team Leader:** Requirements gathering, use cases, system architecture, GitHub coordination, task distribution, progress monitoring, and project integration.
- **Database Developer:** ERD design, database schema, SQL Server setup, EF Core migrations, and seed data.
- **Backend Developer – Authentication & Security:** Authentication, authorization, user roles, doctor verification, access requests, and verification-code logic.
- **Backend Developer – Medical System:** Patient profiles, medical history management, CRUD operations, patient search, medical visits, and audit logs.
- **Frontend Developer:** UI design, responsive layouts, dashboards, user interfaces, and frontend-backend integration.

*Responsibilities may overlap depending on project requirements and team progress.*

## Key Performance Indicators (KPIs)

- Completion of all agreed-upon core project features.
- Successful implementation of authentication and authorization.
- Successful implementation of patient-controlled access permissions.
- Successful CRUD operations for patient profiles and medical history.
- Accurate retrieval of patient records through the search functionality.
- Successful implementation of doctor verification and access requests.
- Successful integration between frontend, backend, and database.
- Completion of testing for the main system functionalities.
- Completion of project documentation and final deliverables within the planned timeline.

## Instructor

**Instructor:** Ayed Ali

## Project Files

This repository contains the CARELINK project source code and related documentation.

### Planned Repository Structure

```text
CARELINK/
│
├── Documentation/
│   ├── Requirements/
│   ├── UseCases/
│   ├── UserFlows/
│   ├── ERD/
│   └── ProjectAnalysis/
│
├── CARELINK/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Data/
│   └── wwwroot/
│
├── Database/
│   ├── Scripts/
│   └── DatabaseDesign/
│
├── README.md
└── .gitignore
```

*The repository structure may change as development progresses.*

## Data Source

The project will use fictional sample records or a suitable, appropriately licensed and de-identified public dataset, if needed, for development and testing.

Any selected dataset will be reviewed for suitability before use. Real patient records and identifiable medical information will not be uploaded to this repository.

## Privacy and Security

CARELINK is designed with medical data privacy in mind. Planned safeguards include:

- Authentication.
- Role-based authorization.
- Input validation.
- Controlled access to patient records.
- Patient consent for medical record access.
- Temporary access permissions.
- Access logging.

The system is an educational prototype and is not intended to replace professional medical judgment or serve as a certified clinical system.

## Project Status

**Status:** Proposal / Initial Development

Features and implementation details may change as the project progresses.

## Future Improvements

- Appointment management and reminders.
- Secure medical document uploads.
- Enhanced reporting and analytics.
- Additional security measures.
- Improved user experience and accessibility.
- Integration with external healthcare systems, subject to feasibility and appropriate approvals.

---

**Program:** Digital Egypt Pioneers Initiative (DEPI)  
**Track:** Full Stack .NET Web Development  
**Project Type:** Capstone Project

## Disclaimer

CARELINK is developed for educational purposes. It does not provide automated diagnoses, and all medical decisions remain the responsibility of qualified healthcare professionals.
