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

        // GET: Home/Details/{id:int?}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DepartmentContact departmentContact = await _context.DepartmentContact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departmentContact == null)
            {
                return NotFound();
            }

            return View(departmentContact);
        }

        // GET: Home/RefreshDataCheck
        public async Task<IActionResult> RefreshDataCheck()
        {
            return View();
        }

        // GET: Home/RefreshData
        public async Task<IActionResult> RefreshData()
        {
            _context.DepartmentContact.RemoveRange(_context.DepartmentContact);
            await _context.SaveChangesAsync();

            List<DepartmentContact> departmentContacts = new List<DepartmentContact>();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetAll");
            HttpClient client = _clientFactory.CreateClient("contacts");
            HttpResponseMessage response = await client.SendAsync(request);
            Message[] contacts = Newtonsoft.Json.JsonConvert.DeserializeObject<Message[]>(await response.Content.ReadAsStringAsync());

            Message[] departments = null;
            try
            {
                request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetAll");
                client = _clientFactory.CreateClient("departments");
                response = await client.SendAsync(request);
                departments = Newtonsoft.Json.JsonConvert.DeserializeObject<Message[]>(await response.Content.ReadAsStringAsync());
            } catch (Exception)
            {
                // Log
            }

            for (int i = 0; i < contacts.Length; i++)
            {
                Message matchingDepartment = null;

                if (departments != null)
                {
                    try
                    {
                        matchingDepartment = departments.Where(d => d.DepartmentId == contacts[i].DepartmentId).First();
                    } catch (Exception)
                    {
                        //Log
                    }
                }

                departmentContacts.Add(new DepartmentContact
                {
                    Id = i,
                    DepartmentId = Int32.Parse(matchingDepartment != null ? matchingDepartment.DepartmentId : contacts[i].DepartmentId),
                    DepartmentName = matchingDepartment != null ? matchingDepartment.DepartmentName : "Unknown Department",
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

        // POST: Home/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Message jsonPerson)
        {
            List<DepartmentContact> departmentContacts = await _context.DepartmentContact.ToListAsync();
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

            return Ok();
        }

        // POST: Home/Update
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Message[] jsonPeople)
        {
            List<DepartmentContact> departmentContacts = await _context.DepartmentContact.ToListAsync();

            foreach (Message p in jsonPeople)
            {
                DepartmentContact departmentContact = departmentContacts.Where(c => c.ContactId == p.Id).First();
                departmentContact.ContactId = p.Id;
                departmentContact.ContactName = p.Name;
                departmentContact.DepartmentId = Int32.Parse(p.DepartmentId);
                departmentContact.DepartmentName = p.DepartmentName;
                departmentContact.LastUpdated = DateTime.Now;
            }

            _context.UpdateRange(departmentContacts);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Home/UpdateEntryDepartment
        [HttpPost]
        public async Task<IActionResult> UpdateEntryDepartment([FromBody] Message jsonPerson)
        {
            IEnumerable<DepartmentContact> contactsToUpdate = _context.DepartmentContact.Where(c => c.DepartmentId == Int32.Parse(jsonPerson.DepartmentId));
            contactsToUpdate.ToList().ForEach(c => c.DepartmentName = jsonPerson.DepartmentName);

            if (ModelState.IsValid)
            {
                _context.UpdateRange(contactsToUpdate);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: Home/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DepartmentContact departmentContact = _context.DepartmentContact.Where(e => e.ContactId == id).First();
            _context.DepartmentContact.Remove(departmentContact);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
