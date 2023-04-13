using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHR.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            //update database if migrations are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                
            }

            // Add Appointment status values if no records exist
            var appointmentStatus = _db.AppointmentStatuses.Count();
            if (appointmentStatus == 0)
            {
                _db.AppointmentStatuses.Add(new AppointmentStatus() { Status = "Approved" });
                _db.AppointmentStatuses.Add(new AppointmentStatus() { Status = "Pending" });
                _db.AppointmentStatuses.Add(new AppointmentStatus() { Status = "Cancelled" });
                _db.SaveChanges();

                // Add Appointment Timeslots if no records exist
                var timeslot = _db.AppointmentTime.Count();
                if (timeslot == 0)
                {
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "8 AM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "9 AM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "10 AM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "11 AM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "1 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "2 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "3 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "4 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "5 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "6 PM" });
                    _db.AppointmentTime.Add(new AppointmentTime() { TimeSlot = "7 PM" });
                }

                // Add Folder Type values if no records exist
                var folderTypes = _db.FolderTypes.Count();
                if (folderTypes == 0)
                {
                    _db.FolderTypes.Add(new FolderType() { Type = "Radiographs" });
                    _db.FolderTypes.Add(new FolderType() { Type = "Photos" });
                    _db.FolderTypes.Add(new FolderType() { Type = "Other Documents" });
                    _db.SaveChanges();
                }

                // Add Payment Status values if no records exist
                var paymentStatus = _db.PaymentStatuses.Count();
                if (paymentStatus == 0)
                {
                    _db.PaymentStatuses.Add(new PaymentStatus() { Status = "Paid" });
                    _db.PaymentStatuses.Add(new PaymentStatus() { Status = "Pending" });
                    _db.SaveChanges();
                }

                // Add Payment Method values if no records exist
                var paymentMethod = _db.PaymentMethods.Count();
                if (paymentMethod == 0)
                {
                    _db.PaymentMethods.Add(new PaymentMethod() { Method = "Cash" });
                    _db.PaymentMethods.Add(new PaymentMethod() { Method = "GCash" });
                    _db.SaveChanges();
                }

                // Add Treatment Type values if no records exist
                var treatmentType = _db.TreatmentTypes.Count();
                if (treatmentType == 0)
                {
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Prosthodontic" });
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Restorative" });
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Orthodontic" });
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Cosmetic" });
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Endodontic" });
                    _db.TreatmentTypes.Add(new TreatmentType() { Type = "Surgical" });
                    _db.SaveChanges();
                }

                // Add Treatment Type values if no records exist
                var toothLabels = _db.ToothLabels.Count();
                if (toothLabels == 0)
                {
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Present", Scope = "Whole", Color = "White", Abbreviation = "&#10003;" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Missing Due To Caries", Scope = "Whole", Color = "White", Abbreviation = "M" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Missing Due To Other Causes", Scope = "Whole", Color = "White", Abbreviation = "Mo" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Impacted Tooth", Scope = "Whole", Color = "White", Abbreviation = "Im" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Root Fragment", Scope = "Whole", Color = "White", Abbreviation = "Rf" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Unerupted", Scope = "Whole", Color = "White", Abbreviation = "Un" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Extraction Due To Caries", Scope = "Whole", Color = "White", Abbreviation = "X" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Extraction Due To Other Causes", Scope = "Whole", Color = "White", Abbreviation = "Xo" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Jacket Crown", Scope = "Whole", Color = "White", Abbreviation = "JC" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Abutment", Scope = "Whole", Color = "White", Abbreviation = "Ab" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Attachment", Scope = "Whole", Color = "White", Abbreviation = "Att" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Pontic", Scope = "Whole", Color = "White", Abbreviation = "P" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Implant", Scope = "Whole", Color = "White", Abbreviation = "Imp" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Amalgam", Scope = "Part", Color = "dark-gray", Abbreviation = "Am" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Composite", Scope = "Part", Color = "blue", Abbreviation = "Co" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Inlay", Scope = "Part", Color = "light-green", Abbreviation = "In" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Onlay", Scope = "Part", Color = "purple", Abbreviation = "On" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Sealant", Scope = "Part", Color = "dark-blue", Abbreviation = "S" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "Caries", Scope = "Part", Color = "red", Abbreviation = "C" });
                    _db.ToothLabels.Add(new ToothLabel() { Label = "None", Scope = "Part", Color = "White", Abbreviation = "N/A" });
                    _db.SaveChanges();
                }

                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole("Dentist")).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole("Assistant")).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole("Owner")).GetAwaiter().GetResult();

                    // if roles are not created, then we will create admin user as well
                    _db.Clinics.Add(new Clinic()
                    {
                        Name = "Initial Clinic",
                        Address = AesOperation.EncryptString("123 Main St."),
                        City = AesOperation.EncryptString("Manila"),
                        Province = AesOperation.EncryptString("Metro Manila"),
                        ZipCode = AesOperation.EncryptString("1234")
                    });
                    _db.SaveChanges();

                    var clinic = _db.Clinics.SingleOrDefault(x => x.Name == "Initial Clinic");

                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "vitasysehrapp@gmail.com",
                        Email = "vitasysehrapp@gmail.com",
                        FirstName = AesOperation.EncryptString("Admin"),
                        LastName = AesOperation.EncryptString("User"),
                        Gender = AesOperation.EncryptString("Prefer not to say"),
                        DOB = AesOperation.EncryptString("01/01/1995"),
                        Address = AesOperation.EncryptString("Barangay 8"),
                        City = AesOperation.EncryptString("Batangas City"),
                        Province = AesOperation.EncryptString("Batangas"),
                        ZipCode = AesOperation.EncryptString("4200"),
                        ClinicId = clinic.Id,
                        EmailConfirmed = true,
                        AdminVerified = "Approved",
                    }, "Admin123*").GetAwaiter().GetResult();

                    clinic.Name = AesOperation.EncryptString(clinic.Name);
                    _db.Clinics.Update(clinic);
                    _db.SaveChanges();

                    ApplicationUser? user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "vitasysehrapp@gmail.com");
                    if (user != null)
                    {
                        _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
                    }
                }

            }
            return;
        }
    }
}
