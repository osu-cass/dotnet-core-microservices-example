using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Deq.Demo.Dept.Web.Models;
using System.Net.Http;
using System.Text;
using Deq.Demo.Shared;
using Microsoft.AspNetCore.Routing;

namespace Deq.Demo.Dept.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DepartmentContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(DepartmentContext context, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _context = context;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            return View(await _context.Department.ToListAsync());
        }

        // GET: Home/ReassignContacts/{id:int}
        public async Task<IActionResult> ReassignContacts(int id)
        {
            return View(id);
        }

        // POST: Home/ReassignContacts/{id:int}/{newId:int}
        [HttpPost]
        public async Task<IActionResult> ReassignContacts(int id, int newId)
        {
            string newName = (await _context.Department.FirstOrDefaultAsync(m => m.Id == newId)).Name;
            IEnumerable<KeyValuePair<string, string>> data = new RouteValueDictionary(new { newId, newName })
                .Select(k => new KeyValuePair<string, string>(k.Key, k.Value?.ToString()));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"Home/ReassignContacts/{id}")
            {
                Content = new FormUrlEncodedContent(data)
            };
            HttpClient client = _clientFactory.CreateClient("contacts");
            HttpResponseMessage response = await client.SendAsync(request);

            Department department = await _context.Department.FindAsync(id);
            _context.Department.Remove(department);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Home/Details/{id:int?}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Home/GetDepartmentName/{id:int?}
        public async Task<IActionResult> GetDepartmentName(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return Ok(department.Name);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Home/GetAll
        public async Task<IActionResult> GetAll()
        {
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(await _context.Department.Select(d =>
            new Message
            {
                DepartmentId = d.Id.ToString(),
                DepartmentName = d.Name
            }).ToArrayAsync()));
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactNumber")] Department department)
        {
            department.LastUpdated = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Home/Edit/{id:int?}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Home/Edit/{id:int?}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactNumber")] Department department)
        {
            department.LastUpdated = DateTime.Now;
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();

                    Message message = new Message()
                    {
                        DepartmentId = department.Id.ToString(),
                        DepartmentName = department.Name
                    };

                    try
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"Home/UpdateEntryDepartment")
                        {
                            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
                        };
                        HttpClient client = _clientFactory.CreateClient("portal");
                        HttpResponseMessage response = await client.SendAsync(request);
                    } catch (Exception)
                    {
                        //Log
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            return View(department);
        }

        // GET: Home/Delete/{id:int?}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Home/DeleteConfirmed/{id:int}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetAssociatedContactsLength/{id}");
            HttpClient client = _clientFactory.CreateClient("contacts");
            HttpResponseMessage response = await client.SendAsync(request);
            int associatedContactsLength = Int32.Parse(await response.Content.ReadAsStringAsync());

            if(associatedContactsLength == 0)
            {
                Department department = await _context.Department.FindAsync(id);
                _context.Department.Remove(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            } else
            {
                return RedirectToAction(nameof(ReassignContacts), new { id });
            }
        }

        // Internal function for checking if a department exists
        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.Id == id);
        }
    }
}
