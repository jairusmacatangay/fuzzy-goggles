using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private ApplicationDbContext _db;

        public SubscriptionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Subscription obj)
        {
            _db.Subscriptions.Update(obj);
        }
    }
}
