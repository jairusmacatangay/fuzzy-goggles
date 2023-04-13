using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository.IRepository
{
    public interface IAllergyRepository : IRepository<Allergy>
    {
        void Update(Allergy obj);
    }
}
