using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class DentalChartRepository : Repository<DentalChart>, IDentalChartRepository
    {
        private ApplicationDbContext _db;

        public DentalChartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(DentalChart obj)
        {
            _db.DentalCharts.Update(obj);
        }
    }
}
