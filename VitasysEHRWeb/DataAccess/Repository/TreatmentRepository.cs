using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class TreatmentRepository : Repository<Treatment>, ITreatmentRepository
    {
        private ApplicationDbContext _db;

        public TreatmentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Treatment obj)
        {
            var objFromDb = _db.Treatments.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.DateAdded = obj.DateAdded;
                objFromDb.LastModified = DateTime.Now;
                objFromDb.Name = obj.Name;
                objFromDb.TreatmentTypeId = obj.TreatmentTypeId;
                objFromDb.Price = obj.Price;
            }
        }
    }
}
