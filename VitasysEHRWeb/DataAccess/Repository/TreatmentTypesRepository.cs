using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class TreatmentTypesRepository: Repository<TreatmentType>, ITreatmentTypesRepository
    {
        private ApplicationDbContext _db;

        public TreatmentTypesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TreatmentType obj)
        {
            _db.TreatmentTypes.Update(obj);
        }
    }
}
