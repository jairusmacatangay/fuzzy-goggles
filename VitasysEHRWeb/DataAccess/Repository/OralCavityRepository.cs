using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class OralCavityRepository : Repository<OralCavity>, IOralCavityRepository
    {
        private ApplicationDbContext _db;

        public OralCavityRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OralCavity obj)
        {
            _db.OralCavities.Update(obj);
        }
    }
}
