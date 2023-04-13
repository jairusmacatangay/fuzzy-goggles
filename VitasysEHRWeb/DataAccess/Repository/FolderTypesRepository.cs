using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class FolderTypesRepository : Repository<FolderType>, IFolderTypesRepository
    {
        private ApplicationDbContext _db;

        public FolderTypesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(FolderType obj)
        {
            _db.FolderTypes.Update(obj);
        }
    }
}
