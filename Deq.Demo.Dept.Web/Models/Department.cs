using System;
using System.Collections.Generic;

namespace Deq.Demo.Dept.Web.Models
{
    public partial class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
