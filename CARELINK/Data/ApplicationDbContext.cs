using CARELINK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CARELINK.Data;

/// <summary>
/// EF Core context for the CARELINK_V2 database.
///
/// The schema is created by the SQL scripts in Database/Scripts
/// (CARELINK_Version2.sql, then CARELINK_V2_Auth_Patch.sql), not by EF migrations,
/// so every mapping here must match those scripts exactly.
///
/// Identity is mapped onto the team's dbo.ApplicationUsers table instead of AspNetUsers,
/// so PatientProfile / DoctorProfile / AuditLogs keep their foreign keys to User_Id.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<ApplicationUser>(options)
{
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<AccessGrant> AccessGrants => Set<AccessGrant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MedicalCondition> MedicalConditions => Set<MedicalCondition>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<Surgery> Surgeries => Set<Surgery>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicalVisit> MedicalVisits => Set<MedicalVisit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // CARELINK does not use external logins, per-user claims or stored tokens,
        // so these Identity tables do not exist in our database.
        builder.Ignore<IdentityUserClaim<string>>();
        builder.Ignore<IdentityUserLogin<string>>();
        builder.Ignore<IdentityUserToken<string>>();

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("ApplicationUsers", "dbo");

            b.Property(u => u.Id).HasColumnName("User_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.Role).HasMaxLength(128).IsRequired();
            b.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            b.Property(u => u.SecurityStamp).HasMaxLength(256);
            b.Property(u => u.ConcurrencyStamp).HasMaxLength(256);

            // Not used in CARELINK, and not columns in ApplicationUsers.
            b.Ignore(u => u.PhoneNumber);
            b.Ignore(u => u.PhoneNumberConfirmed);
            b.Ignore(u => u.TwoFactorEnabled);

            b.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UQ_ApplicationUsers_Email");
            b.HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasFilter("[NormalizedUserName] IS NOT NULL")
                .HasDatabaseName("UX_ApplicationUsers_NormalizedUserName");
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("IX_ApplicationUsers_NormalizedEmail");
        });

        builder.Entity<PatientProfile>(b =>
        {
            b.ToTable("PatientProfile", "dbo");
            b.HasKey(p => p.PatientId);
            b.Property(p => p.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(p => p.UserId).HasColumnName("User_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(p => p.FullName).HasMaxLength(150);
            b.Property(p => p.NationalIdEncrypted).HasMaxLength(256);
            b.HasIndex(p => p.UserId).IsUnique();
            b.HasOne(p => p.User).WithOne().HasForeignKey<PatientProfile>(p => p.UserId);
        });

        builder.Entity<DoctorProfile>(b =>
        {
            b.ToTable("DoctorProfile", "dbo");
            b.HasKey(d => d.DoctorId);
            b.Property(d => d.DoctorId).HasColumnName("Doctor_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(d => d.UserId).HasColumnName("User_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(d => d.FullName).HasMaxLength(150);
            b.Property(d => d.Specialty).HasMaxLength(256);
            b.Property(d => d.LicenseNumber).HasMaxLength(256);
            b.Property(d => d.IsApproved).HasColumnName("Is_Approved");
            b.HasIndex(d => d.UserId).IsUnique();
            b.HasIndex(d => d.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
            b.HasOne(d => d.User).WithOne().HasForeignKey<DoctorProfile>(d => d.UserId);
        });

        builder.Entity<AccessRequest>(b =>
        {
            b.ToTable("AccessRequests", "dbo");
            b.HasKey(r => r.RequestId);
            b.Property(r => r.RequestId).HasColumnName("Request_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(r => r.DoctorId).HasColumnName("Doctor_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(r => r.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(r => r.Status).HasMaxLength(50);
            b.HasOne(r => r.Doctor).WithMany().HasForeignKey(r => r.DoctorId);
            b.HasOne(r => r.Patient).WithMany().HasForeignKey(r => r.PatientId);
            b.HasOne(r => r.Grant).WithOne(g => g.Request).HasForeignKey<AccessGrant>(g => g.RequestId);
        });

        builder.Entity<AccessGrant>(b =>
        {
            b.ToTable("AccessGrants", "dbo");
            b.HasKey(g => g.AccessId);
            b.Property(g => g.AccessId).HasColumnName("Access_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(g => g.RequestId).HasColumnName("Request_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(g => g.CodeHash).HasMaxLength(256);
            b.HasIndex(g => g.RequestId).IsUnique();
        });

        builder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs", "dbo");
            b.HasKey(a => a.AuditId);
            b.Property(a => a.AuditId).HasColumnName("Audit_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(a => a.UserId).HasColumnName("User_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(a => a.Action).HasMaxLength(256);
            b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId);
        });

        builder.Entity<MedicalCondition>(b =>
        {
            b.ToTable("MedicalConditions", "dbo");
            b.HasKey(c => c.ConditionId);
            b.Property(c => c.ConditionId).HasColumnName("Condition_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(c => c.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(c => c.ConditionName).HasMaxLength(150);
            b.Property(c => c.Notes).HasColumnName("Notes_Condition").HasMaxLength(500);
            b.HasOne(c => c.Patient).WithMany().HasForeignKey(c => c.PatientId);
        });

        builder.Entity<Allergy>(b =>
        {
            b.ToTable("Allergies", "dbo");
            b.HasKey(a => a.AllergyId);
            b.Property(a => a.AllergyId).HasColumnName("Allergy_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(a => a.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(a => a.AllergyName).HasMaxLength(150);
            b.Property(a => a.Reaction).HasMaxLength(256);
            b.Property(a => a.Severity).HasMaxLength(50);
            b.Property(a => a.Notes).HasColumnName("Notes_Allergies").HasMaxLength(500);
            b.HasOne(a => a.Patient).WithMany().HasForeignKey(a => a.PatientId);
        });

        builder.Entity<Surgery>(b =>
        {
            b.ToTable("Surgeries", "dbo");
            b.HasKey(s => s.SurgeryId);
            b.Property(s => s.SurgeryId).HasColumnName("Surgery_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(s => s.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(s => s.SurgeryName).HasMaxLength(150);
            b.Property(s => s.Hospital).HasMaxLength(150);
            b.Property(s => s.Notes).HasColumnName("Notes_Surgeries").HasMaxLength(500);
            b.HasOne(s => s.Patient).WithMany().HasForeignKey(s => s.PatientId);
        });

        builder.Entity<Medication>(b =>
        {
            b.ToTable("Medications", "dbo");
            b.HasKey(m => m.MedicationId);
            b.Property(m => m.MedicationId).HasColumnName("Medication_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(m => m.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(m => m.MedicationName).HasMaxLength(150);
            b.Property(m => m.Dosage).HasMaxLength(100);
            b.Property(m => m.Frequency).HasMaxLength(100);
            b.Property(m => m.Notes).HasColumnName("Notes_Medications").HasMaxLength(500);
            b.HasOne(m => m.Patient).WithMany().HasForeignKey(m => m.PatientId);
        });

        builder.Entity<MedicalVisit>(b =>
        {
            b.ToTable("MedicalVisits", "dbo");
            b.HasKey(v => v.VisitId);
            b.Property(v => v.VisitId).HasColumnName("Visit_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(v => v.PatientId).HasColumnName("Patient_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(v => v.DoctorId).HasColumnName("Doctor_Id").HasMaxLength(20).IsUnicode(false);
            b.Property(v => v.Diagnosis).HasMaxLength(500);
            b.Property(v => v.Treatment).HasMaxLength(500);
            b.Property(v => v.Notes).HasColumnName("Notes_Visits").HasMaxLength(500);
            b.HasOne(v => v.Patient).WithMany().HasForeignKey(v => v.PatientId);
            b.HasOne(v => v.Doctor).WithMany().HasForeignKey(v => v.DoctorId);
        });
    }
}
