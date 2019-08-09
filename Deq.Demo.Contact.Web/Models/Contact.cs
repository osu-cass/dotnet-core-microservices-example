using System;
using System.Collections.Generic;

namespace Deq.Demo.Contact.Web.Models
{
    public partial class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime LastUpdated { get; set;  }
    }
}
