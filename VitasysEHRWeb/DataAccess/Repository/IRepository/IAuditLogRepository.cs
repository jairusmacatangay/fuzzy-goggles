using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository.IRepository
{
    public interface IAuditLogRepository: IRepository<AuditLog>
    {
        void Update(AuditLog obj);
    }
}
