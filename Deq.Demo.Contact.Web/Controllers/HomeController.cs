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

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
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

        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactNumber,DateOfBirth,DepartmentId")] Models.Contact contact)
        {
            contact.LastUpdated = DateTime.Now;

            var request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetDepartmentName/{contact.DepartmentId}");

            var client = _clientFactory.CreateClient("departments");

            var response = await client.SendAsync(request);
            contact.DepartmentName = await response.Content.ReadAsStringAsync();

            if (ModelState.IsValid)
            {
                ContactMessage message = new ContactMessage
                {
                    ContactNumber = contact.ContactNumber,
                    Name = contact.Name,
                    Id = contact.Id,
                    DepartmentId = contact.DepartmentId,
                    DepartmentName = contact.DepartmentName
                };

                request = new HttpRequestMessage(HttpMethod.Post, $"Home/Create")
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
                };

                client = _clientFactory.CreateClient("portal");

                response = await client.SendAsync(request);

                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(contact);
        }

        // GET: Home/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactNumber,DateOfBirth,DepartmentId")] Models.Contact contact)
        {
            contact.LastUpdated = DateTime.Now;

            var request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetDepartmentName/{contact.DepartmentId}");

            var client = _clientFactory.CreateClient("departments");

            var response = await client.SendAsync(request);
            contact.DepartmentName = await response.Content.ReadAsStringAsync();

            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    ContactMessage message = new ContactMessage
                    {
                        ContactNumber = contact.ContactNumber,
                        Name = contact.Name,
                        Id = contact.Id,
                        DepartmentId = contact.DepartmentId,
                        DepartmentName = contact.DepartmentName
                    };
                    request = new HttpRequestMessage(HttpMethod.Post, $"Home/Update")
                    {
                        Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
                    };
                    client = _clientFactory.CreateClient("portal");
                    response = await client.SendAsync(request);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Home/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contact.FindAsync(id);
            _context.Contact.Remove(contact);
            await _context.SaveChangesAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, $"Home/Delete/{contact.Id}");
            var client = _clientFactory.CreateClient("portal");
            var response = await client.SendAsync(request);

            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contact.Any(e => e.Id == id);
        }
    }
}
