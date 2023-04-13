using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class AllergyRepository : Repository<Allergy>, IAllergyRepository
    {
        private ApplicationDbContext _db;

        public AllergyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Allergy obj)
        {
            _db.Allergies.Update(obj);
        }
    }
}
