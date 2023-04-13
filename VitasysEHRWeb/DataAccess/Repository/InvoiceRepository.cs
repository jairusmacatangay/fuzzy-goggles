using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        private ApplicationDbContext _db;

        public InvoiceRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Invoice obj)
        {
            _db.Invoices.Update(obj);
        }
    }
}
