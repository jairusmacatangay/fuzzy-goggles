using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ReviewOfSystemRepository : Repository<ReviewOfSystem>, IReviewOfSystemRepository
    {
        private ApplicationDbContext _db;

        public ReviewOfSystemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReviewOfSystem obj)
        {
            _db.ReviewOfSystems.Update(obj);
        }
    }
}
