using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class AuditLogPatientRepository : Repository<AuditLogPatient>, IAuditLogPatientRepository
    {
        private ApplicationDbContext _db;

        public AuditLogPatientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(AuditLogPatient obj)
        {
            _db.AuditLogPatients.Update(obj);
        }
    }
}
