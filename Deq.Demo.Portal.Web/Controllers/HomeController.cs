using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Deq.Demo.Portal.Web.Models;
using System.Net.Http;
using Deq.Demo.Shared;

namespace Deq.Demo.Portal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DepartmentContactContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(DepartmentContactContext context)
        {
            _context = context;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            return View(await _context.DepartmentContact.ToListAsync());
        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departmentContact = await _context.DepartmentContact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departmentContact == null)
            {
                return NotFound();
            }

            return View(departmentContact);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Home/SetPerson
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContactMessage jsonPerson)
        {
            var departmentContacts = await _context.DepartmentContact.ToListAsync();
            DepartmentContact departmentContact = new DepartmentContact
            {
                Id = departmentContacts.Count,
                ContactId = jsonPerson.Id,
                ContactName = jsonPerson.Name,
                DepartmentId = Int32.Parse(jsonPerson.DepartmentId),
                DepartmentName = jsonPerson.DepartmentName,
                LastUpdated = DateTime.Now
            };

            if (ModelState.IsValid)
            {
                _context.Add(departmentContact);
                await _context.SaveChangesAsync();
            }

            return new OkResult();
        }

        // POST: Home/Update
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ContactMessage[] jsonPeople)
        {
            var departmentContacts = await _context.DepartmentContact.ToListAsync();

            foreach (var p in jsonPeople)
            {
                var departmentContact = departmentContacts.Where(c => c.ContactId == p.Id).First();
                departmentContact.ContactId = p.Id;
                departmentContact.ContactName = p.Name;
                departmentContact.DepartmentId = Int32.Parse(p.DepartmentId);
                departmentContact.DepartmentName = p.DepartmentName;
                departmentContact.LastUpdated = DateTime.Now;
            }

            _context.UpdateRange(departmentContacts);
            await _context.SaveChangesAsync();

            return new OkResult();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEntryDepartment([FromBody] ContactMessage jsonPerson)
        {
            IEnumerable<DepartmentContact> contactsToUpdate = _context.DepartmentContact.Where(c => c.DepartmentId == Int32.Parse(jsonPerson.DepartmentId));
            contactsToUpdate.ToList().ForEach(c => c.DepartmentName = jsonPerson.DepartmentName);

            if (ModelState.IsValid)
            {
                _context.UpdateRange(contactsToUpdate);
                await _context.SaveChangesAsync();
            }

            return new OkResult();
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var departmentContact = _context.DepartmentContact.Where(e => e.ContactId == id).First();
            _context.DepartmentContact.Remove(departmentContact);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentContactExists(int id)
        {
            return _context.DepartmentContact.Any(e => e.Id == id);
        }
    }
}
