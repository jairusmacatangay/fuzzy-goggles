using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ReminderRepository : Repository<Reminder>, IReminderRepository
    {
        private ApplicationDbContext _db;

        public ReminderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Reminder obj)
        {
            _db.Reminders.Update(obj);
        }
    }
}
