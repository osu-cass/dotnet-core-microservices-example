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

        public HomeController(DepartmentContactContext context, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
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

        public async Task<IActionResult> RefreshDataCheck()
        {
            return View();
        }

        public async Task<IActionResult> RefreshData()
        {
            _context.DepartmentContact.RemoveRange(_context.DepartmentContact);
            await _context.SaveChangesAsync();

            List<DepartmentContact> departmentContacts = new List<DepartmentContact>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetAll");
            var client = _clientFactory.CreateClient("contacts");
            var response = await client.SendAsync(request);
            ContactMessage[] contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactMessage[]>(await response.Content.ReadAsStringAsync());

            for(int i = 0; i < contacts.Length; i++)
            {
                departmentContacts.Add(new DepartmentContact {
                    Id = i,
                    DepartmentId = Int32.Parse(contacts[i].DepartmentId),
                    DepartmentName = contacts[i].DepartmentName,
                    ContactId = contacts[i].Id,
                    ContactName = contacts[i].Name,
                    LastUpdated = DateTime.Now
                });
            }

            _context.AddRange(departmentContacts);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Home/SetPerson
        [Route("/Home/Create")]
        [ProducesResponseType(200)]
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
        [Route("/Home/Update")]
        [ProducesResponseType(200)]
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

        [Route("/Home/UpdateEntryDepartment")]
        [ProducesResponseType(200)]
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

        [Route("/Home/DeleteConfirmed")]
        [ProducesResponseType(302)]
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
