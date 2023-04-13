using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        private ApplicationDbContext _db;

        public AuditLogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(AuditLog obj)
        {
            _db.AuditLogs.Update(obj);
        }
    }
}
