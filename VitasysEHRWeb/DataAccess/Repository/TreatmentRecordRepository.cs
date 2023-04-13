using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class TreatmentRecordRepository : Repository<TreatmentRecord>, ITreatmentRecordRepository
    {
        private ApplicationDbContext _db;

        public TreatmentRecordRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TreatmentRecord obj)
        {
            _db.TreatmentRecords.Update(obj);
        }
    }
}
