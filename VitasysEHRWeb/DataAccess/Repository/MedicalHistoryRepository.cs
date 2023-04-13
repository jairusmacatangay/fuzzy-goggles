using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class MedicalHistoryRepository : Repository<MedicalHistory>, IMedicalHistoryRepository
    {
        private ApplicationDbContext _db;

        public MedicalHistoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(MedicalHistory obj)
        {
            _db.MedicalHistories.Update(obj);
        }
    }
}
