using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        private ApplicationDbContext _db;

        public PatientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Patient obj)
        {
            var objFromDb = _db.Patients.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.FirstName = obj.FirstName;
                objFromDb.MiddleName = obj.MiddleName;
                objFromDb.LastName = obj.LastName;
                objFromDb.DOB = obj.DOB;
                objFromDb.Email = obj.Email;
                objFromDb.HomeNumber = obj.HomeNumber;
                objFromDb.MobileNumber = obj.MobileNumber;
                objFromDb.Gender = obj.Gender;
                objFromDb.Address = obj.Address;
                objFromDb.City = obj.City;
                objFromDb.Province = obj.Province;
                objFromDb.ZipCode = obj.ZipCode;
                objFromDb.LastModified = DateTime.Now;
                if (obj.ProfPicUrl != null)
                {
                    objFromDb.ProfPicUrl = obj.ProfPicUrl;
                }
            }
        }
    }
}
