using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        private ApplicationDbContext _db;

        public DocumentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Document obj)
        {
            var objFromDb = _db.Documents.FirstOrDefault(x => x.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.DocumentUrl = obj.DocumentUrl;
                objFromDb.Name = obj.Name;
                objFromDb.IsShared = obj.IsShared;
                objFromDb.IsArchived = obj.IsArchived;
                objFromDb.FolderId = obj.FolderId;
                objFromDb.LastModified = DateTime.Now;
            }
        }
    }
}
