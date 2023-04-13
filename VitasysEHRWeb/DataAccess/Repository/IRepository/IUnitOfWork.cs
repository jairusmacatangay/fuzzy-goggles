using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAllergyRepository Allergy { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IAppointmentRepository Appointment { get; }
        IAuditLogRepository AuditLog { get; }
        IClinicRepository Clinic { get; }
        IDentalChartRepository DentalChart { get; }
        IDocumentRepository Document { get; }
        IFolderRepository Folder { get; }
        IFolderTypesRepository FolderType { get; }
        IInvoiceRepository Invoice { get; }
        IMedicalHistoryRepository MedicalHistory { get; }
        IOralCavityRepository OralCavity { get; }
        IPatientRepository Patient { get; }
        IPaymentRepository Payment { get; }
        IPrescriptionRepository Prescription { get; }
        IReviewOfSystemRepository ReviewOfSystem { get; }
        ISubscriptionRepository Subscription { get; }
        IToothRepository ToothDetail { get; }
        ITreatmentRepository Treatment { get; }
        ITreatmentTypesRepository TreatmentType { get; }
        ITreatmentRecordRepository TreatmentRecord { get; }
        IClinicPatientRepository ClinicPatient { get; }
        IAuditLogPatientRepository AuditLogPatient { get; }
        IPaymentMethodRepository PaymentMethod { get; }
        IAppointmentTimeRepository AppointmentTime { get; }
        IReminderRepository Reminder { get; }
        IToothLabelRepository ToothLabel { get; }

        void Save();
    }
}
