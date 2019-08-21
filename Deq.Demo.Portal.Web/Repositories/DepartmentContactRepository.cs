using Deq.Demo.Portal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deq.Demo.Portal.Web.Repositories
{
    public class DepartmentContactRepository : RepositoryBase<DepartmentContact, int>
    {
        public DepartmentContactRepository(DepartmentContactContext context) : base(context) { }

        public IEnumerable<DepartmentContact> GetDepartmentContactsOnDepartmentId(int id)
        {
            return _dbSet.Where(c => c.DepartmentId == id);
        }

        public DepartmentContact GetDepartmentContactsOnContactId(int id)
        {
            return _dbSet.Where(e => e.ContactId == id).First();
        }
    }
}
