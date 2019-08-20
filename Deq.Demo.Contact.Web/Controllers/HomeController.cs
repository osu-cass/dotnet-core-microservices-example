using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Deq.Demo.Contact.Web.Models;
using System.Net.Http;
using System.Text;
using Deq.Demo.Shared;
using Microsoft.AspNetCore.Http;

namespace Deq.Demo.Contact.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ContactContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(ContactContext context, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _context = context;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            return View(await _context.Contact.ToListAsync());
        }

        // GET: Home/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Models.Contact contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Home/GetAll
        public async Task<IActionResult> GetAll()
        {
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(await _context.Contact.Select(c => 
            new Message
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId
            }).ToArrayAsync()));
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactNumber,DateOfBirth,DepartmentId")] Models.Contact contact)
        {
            contact.LastUpdated = DateTime.Now;

            if (ModelState.IsValid)
            {
                Message message = new Message
                {
                    ContactNumber = contact.ContactNumber,
                    Name = contact.Name,
                    Id = contact.Id,
                    DepartmentId = contact.DepartmentId
                };

                try {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"Home/Create")
                    {
                        Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
                    };
                    HttpClient client = _clientFactory.CreateClient("portal");
                    HttpResponseMessage response = await client.SendAsync(request);
                } catch (Exception)
                {
                    // Log
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(contact);
        }

        // GET: Home/Edit/id:int
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Models.Contact contact = await _context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }

        // POST: Home/ReassignContacts/{id:int}
        [HttpPost]
        public async Task<IActionResult> ReassignContacts(int id, [FromForm] string newId, [FromForm] string newName)
        {
            IEnumerable<Models.Contact> contactsToUpdate = _context.Contact.Where(c => c.DepartmentId == id.ToString());
            contactsToUpdate.ToList().ForEach(i => {
                i.DepartmentId = newId.ToString();
            });

            return await EditList(contactsToUpdate.Select(c => (c.Id, c)));
        }

        // POST: Home/Edit/{id:int}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Models.Contact contact)
        {
            contact.LastUpdated = DateTime.Now;

            if (ModelState.IsValid)
            {
                return await EditList(
                    new List<(int, Models.Contact)>
                    {
                        (id, contact)
                    }
                );
            }
            else
            {
                return View(contact);
            }
        }

        // POST: Home/EditList/{id:int}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditList(IEnumerable<(int id, Models.Contact contact)> contactList)
        {
            foreach ((int id, Models.Contact contact) in contactList)
            {
                if (id != contact.Id)
                {
                    return NotFound();
                }

                contact.LastUpdated = DateTime.Now;
            }

            Message[] messageList = contactList.Select(c => new Message
            {
                ContactNumber = c.contact.ContactNumber,
                Name = c.contact.Name,
                Id = c.contact.Id,
                DepartmentId = c.contact.DepartmentId
            }).ToArray();

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"Home/Update")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(messageList), Encoding.UTF8, "application/json")
                };
                HttpClient client = _clientFactory.CreateClient("portal");
                HttpResponseMessage response = await client.SendAsync(request);
            } catch (Exception)
            {
                //Log
            }

            _context.UpdateRange(contactList.Select(c => c.contact));
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Home/GetAssociatedContactsLength/{id:int}
        public async Task<IActionResult> GetAssociatedContactsLength(int id)
        {
            return Ok((await _context.Contact.Where(c => c.DepartmentId == id.ToString()).ToListAsync()).Count());
        }

        // GET: Home/Delete/{id:int?}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Models.Contact contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Home/Delete/{id:int}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Models.Contact contact = await _context.Contact.FindAsync(id);
            _context.Contact.Remove(contact);
            await _context.SaveChangesAsync();

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"Home/Delete/{contact.Id}");
                HttpClient client = _clientFactory.CreateClient("portal");
                HttpResponseMessage response = await client.SendAsync(request);
            } catch (Exception)
            {
                //Log
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
