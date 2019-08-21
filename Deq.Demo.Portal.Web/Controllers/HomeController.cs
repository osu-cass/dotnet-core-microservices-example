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
using Deq.Demo.Portal.Web.Repositories;

namespace Deq.Demo.Portal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DepartmentContactRepository _repository;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(DepartmentContactRepository repository, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _repository = repository;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            return View(await _repository.GetAsync().ConfigureAwait(false));
        }

        // GET: Home/Details/{id:int?}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DepartmentContact departmentContact = await _repository.GetByIDAsync(id.Value).ConfigureAwait(false);
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
            _repository.DeleteAll();
            await _repository.SaveChangesAsync();

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

            _repository.InsertRange(departmentContacts);
            await _repository.SaveChangesAsync();

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
            List<DepartmentContact> departmentContacts = await _repository.GetAsync().ConfigureAwait(false);

            string matchingDepartmentName = null;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetDepartmentName/{jsonPerson.DepartmentId}");
                HttpClient client = _clientFactory.CreateClient("departments");
                HttpResponseMessage response = await client.SendAsync(request);
                matchingDepartmentName = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                // Log
            }

            DepartmentContact departmentContact = new DepartmentContact
            {
                Id = departmentContacts.Count,
                ContactId = jsonPerson.Id,
                ContactName = jsonPerson.Name,
                DepartmentId = Int32.Parse(jsonPerson.DepartmentId),
                DepartmentName = matchingDepartmentName ?? "Unknown Department",
                LastUpdated = DateTime.Now
            };

            if (ModelState.IsValid)
            {
                _repository.Insert(departmentContact);
                await _repository.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: Home/Update
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Message[] jsonPeople)
        {
            List<DepartmentContact> departmentContacts = await _repository.GetAsync().ConfigureAwait(false);

            foreach (Message p in jsonPeople)
            {
                string matchingDepartmentName = null;
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"Home/GetDepartmentName/{p.DepartmentId}");
                    HttpClient client = _clientFactory.CreateClient("departments");
                    HttpResponseMessage response = await client.SendAsync(request);
                    matchingDepartmentName = await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    // Log
                }

                DepartmentContact departmentContact = departmentContacts.Where(c => c.ContactId == p.Id).First();
                departmentContact.ContactId = p.Id;
                departmentContact.ContactName = p.Name;
                departmentContact.DepartmentId = Int32.Parse(p.DepartmentId);
                departmentContact.DepartmentName = matchingDepartmentName ?? "Unknown Department";
                departmentContact.LastUpdated = DateTime.Now;
            }

            _repository.UpdateRange(departmentContacts);
            await _repository.SaveChangesAsync();

            return Ok();
        }

        // POST: Home/UpdateEntryDepartment
        [HttpPost]
        public async Task<IActionResult> UpdateEntryDepartment([FromBody] Message jsonPerson)
        {
            IEnumerable<DepartmentContact> contactsToUpdate = _repository.GetDepartmentContactsOnDepartmentId(Int32.Parse(jsonPerson.DepartmentId));
            contactsToUpdate.ToList().ForEach(c => c.DepartmentName = jsonPerson.DepartmentName);

            if (ModelState.IsValid)
            {
                _repository.UpdateRange(contactsToUpdate);
                await _repository.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: Home/DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DepartmentContact departmentContact = _repository.GetDepartmentContactsOnContactId(id);
            _repository.Delete(departmentContact);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
