using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class FolderRepository : Repository<Folder>, IFolderRepository
    {
        private ApplicationDbContext _db;

        public FolderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Folder obj)
        {
            var objFromDb = _db.Folders.FirstOrDefault(x => x.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.FolderTypeId = obj.FolderTypeId;
                objFromDb.LastModified = DateTime.Now;
            }
        }
    }
}
