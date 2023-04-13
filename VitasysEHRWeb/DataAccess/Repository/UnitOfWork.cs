using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;

namespace VitasysEHR.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Allergy = new AllergyRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            Appointment = new AppointmentRepository(_db);
            AuditLog = new AuditLogRepository(_db);
            Clinic = new ClinicRepository(_db);
            DentalChart = new DentalChartRepository(_db);
            Document = new DocumentRepository(_db);
            Folder = new FolderRepository(_db);
            FolderType = new FolderTypesRepository(_db);
            Invoice = new InvoiceRepository(_db);
            MedicalHistory = new MedicalHistoryRepository(_db);
            OralCavity = new OralCavityRepository(_db);
            Patient = new PatientRepository(_db);
            Payment = new PaymentRepository(_db);
            Prescription = new PrescriptionRepository(_db);
            ReviewOfSystem = new ReviewOfSystemRepository(_db);
            Subscription = new SubscriptionRepository(_db);
            ToothDetail = new ToothRepository(_db);
            ToothDetail = new ToothRepository(_db);
            TreatmentRecord = new TreatmentRecordRepository(_db);
            Treatment = new TreatmentRepository(_db);
            TreatmentType = new TreatmentTypesRepository(_db);
            ClinicPatient = new ClinicPatientRepository(_db);
            AuditLogPatient = new AuditLogPatientRepository(_db);
            PaymentMethod = new PaymentMethodRepository(_db);
            AppointmentTime = new AppointmentTimeRepository(_db);
            Reminder = new ReminderRepository(_db);
            ToothLabel = new ToothLabelRepository(_db);
        }

        public IAllergyRepository Allergy { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IAppointmentRepository Appointment { get; private set; }
        public IAuditLogRepository AuditLog { get; private set; }
        public IClinicRepository Clinic { get; private set; }
        public IDentalChartRepository DentalChart { get; private set; }
        public IDocumentRepository Document { get; private set; }
        public IFolderRepository Folder { get; private set; }
        public IFolderTypesRepository FolderType { get; private set; }
        public IInvoiceRepository Invoice { get; private set; }
        public IMedicalHistoryRepository MedicalHistory { get; private set; }
        public IOralCavityRepository OralCavity { get; private set; }
        public IPatientRepository Patient { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IPrescriptionRepository Prescription { get; private set; }
        public IReviewOfSystemRepository ReviewOfSystem { get; private set; }
        public ISubscriptionRepository Subscription { get; private set; }
        public IToothRepository ToothDetail { get; private set; }
        public ITreatmentRecordRepository TreatmentRecord { get; private set; }
        public ITreatmentRepository Treatment { get; private set; }
        public ITreatmentTypesRepository TreatmentType { get; private set;}
        public IClinicPatientRepository ClinicPatient { get; private set; }
        public IAuditLogPatientRepository AuditLogPatient { get; private set; }
        public IPaymentMethodRepository PaymentMethod { get; private set; }
        public IAppointmentTimeRepository AppointmentTime { get; private set; }
        public IReminderRepository Reminder { get; private set; }
        public IToothLabelRepository ToothLabel { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
