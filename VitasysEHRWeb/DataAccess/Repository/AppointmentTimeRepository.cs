﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class AppointmentTimeRepository : Repository<AppointmentTime>, IAppointmentTimeRepository
    {
        private ApplicationDbContext _db;

        public AppointmentTimeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
