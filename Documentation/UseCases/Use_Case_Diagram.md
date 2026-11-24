# CARELINK – Use Case Diagram

This diagram illustrates the main interactions between CARELINK users and the system.

```mermaid
flowchart LR
    Patient([Patient])
    Doctor([Doctor])
    Admin([Admin])

    subgraph CARELINK System
        P1(Register)
        P2(Login)
        P3(View Profile)
        P4(Manage Medical History)
        P5(Approve or Reject Access)
        P6(Revoke Access)

        D1(Register)
        D2(Login)
        D3(Search Patient)
        D4(Request Access)
        D5(Enter Verification Code)
        D6(View Authorized Medical History)

        A1(Login)
        A2(Verify Doctor Accounts)
        A3(Manage Users and Roles)
        A4(View Audit Logs)
    end

    Patient --- P1
    Patient --- P2
    Patient --- P3
    Patient --- P4
    Patient --- P5
    Patient --- P6

    Doctor --- D1
    Doctor --- D2
    Doctor --- D3
    Doctor --- D4
    Doctor --- D5
    Doctor --- D6

    Admin --- A1
    Admin --- A2
    Admin --- A3
    Admin --- A4
