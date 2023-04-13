using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ToothLabelRepository : Repository<ToothLabel>, IToothLabelRepository
    {
        private ApplicationDbContext _db;

        public ToothLabelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
