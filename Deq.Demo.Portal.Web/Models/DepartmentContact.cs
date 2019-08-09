using System;
using System.Collections.Generic;

namespace Deq.Demo.Portal.Web.Models
{
    public partial class DepartmentContact
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
