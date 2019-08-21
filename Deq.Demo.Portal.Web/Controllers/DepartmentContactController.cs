using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Deq.Demo.Portal.Web.Models;
using Deq.Demo.Portal.Web.Repositories;
using Deq.Demo.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deq.Demo.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentContactController : ControllerBase
    {
        private readonly DepartmentContactRepository _repository;
        private readonly IHttpClientFactory _clientFactory;

        public DepartmentContactController(DepartmentContactRepository repository, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _repository = repository;
        }

        // GET: api/DepartmentContact
        [HttpGet]
        public async Task<IActionResult> Get()
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
            }
            catch (Exception)
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
                    }
                    catch (Exception)
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

            return Redirect("/");
        }

        // POST: api/DepartmentContact
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Message jsonPerson)
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

        // PUT: api/DepartmentContact
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Message[] jsonPeople)
        {
            if (jsonPeople[0].DepartmentName == null || jsonPeople[0].DepartmentName == "")
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
            } else {
                IEnumerable<DepartmentContact> contactsToUpdate = _repository.GetDepartmentContactsOnDepartmentId(Int32.Parse(jsonPeople[0].DepartmentId));
                contactsToUpdate.ToList().ForEach(c => c.DepartmentName = jsonPeople[0].DepartmentName);

                if (ModelState.IsValid)
                {
                    _repository.UpdateRange(contactsToUpdate);
                    await _repository.SaveChangesAsync();
                }

                return Ok();
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            DepartmentContact departmentContact = _repository.GetDepartmentContactsOnContactId(id);
            _repository.Delete(departmentContact);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(HomeController.Index));
        }
    }
}
