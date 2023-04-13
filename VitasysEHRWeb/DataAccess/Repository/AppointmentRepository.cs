using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private ApplicationDbContext _db;

        public AppointmentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Appointment obj)
        {
            _db.Appointments.Update(obj);
        }
    }
}
