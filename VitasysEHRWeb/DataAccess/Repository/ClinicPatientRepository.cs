using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ClinicPatientRepository : Repository<ClinicPatient>, IClinicPatientRepository
    {
        private ApplicationDbContext _db;

        public ClinicPatientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ClinicPatient obj)
        {
            _db.ClinicPatients.Update(obj);
        }
    }
}
