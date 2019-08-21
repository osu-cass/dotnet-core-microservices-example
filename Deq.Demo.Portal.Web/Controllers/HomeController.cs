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
        public IActionResult RefreshDataCheck()
        {
            return View();
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }
    }
}
