using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ToothRepository : Repository<Tooth>, IToothRepository
    {
        private ApplicationDbContext _db;

        public ToothRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Tooth obj)
        {
            _db.Tooth.Update(obj);
        }
    }
}
