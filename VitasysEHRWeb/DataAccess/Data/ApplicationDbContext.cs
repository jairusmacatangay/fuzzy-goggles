using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<TreatmentType> TreatmentTypes { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
        public DbSet<OralCavity> OralCavities { get; set; }
        public DbSet<FolderType> FolderTypes { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<ReviewOfSystem> ReviewOfSystems { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<DentalChart> DentalCharts { get; set; }
        public DbSet<Tooth> Tooth { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<ClinicPatient> ClinicPatients { get; set; }
        public DbSet<AuditLogPatient> AuditLogPatients { get; set; }
        public DbSet<AppointmentTime> AppointmentTime { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<ToothLabel> ToothLabels { get; set; }
    }
}
