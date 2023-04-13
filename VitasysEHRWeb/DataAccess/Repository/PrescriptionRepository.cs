using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
    {
        private ApplicationDbContext _db;

        public PrescriptionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Prescription obj)
        {
            _db.Prescriptions.Update(obj);
        }
    }
}
