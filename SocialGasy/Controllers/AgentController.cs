using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SocialGasy.Models; // Zava-dehibe ity mba hahitana ny modely User

namespace SocialGasy.Controllers
{
    public class AgentController : Controller
    {
        // Mampiasa User fa tsy Agent
        private static List<User> _agents = new List<User>();

        public IActionResult Dashboard() => View(_agents);

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(User agent) 
        {
            // Ny MongoDB dia mampiasa string Id, ka mampiasà GUID
            agent.Id = Guid.NewGuid().ToString(); 
            _agents.Add(agent);
            return RedirectToAction("Dashboard");
        }

        public IActionResult Details(string id) => View(_agents.FirstOrDefault(a => a.Id == id));

        public IActionResult Edit(string id) => View(_agents.FirstOrDefault(a => a.Id == id));

        [HttpPost]
        public IActionResult Edit(User agent) 
        {
            var existing = _agents.FirstOrDefault(a => a.Id == agent.Id);
            if (existing != null) 
            {
                existing.FullName = agent.FullName;
                existing.Cin = agent.Cin;
                existing.Address = agent.Address;
                existing.PhoneNumber = agent.PhoneNumber;
                existing.Email = agent.Email;
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Delete(string id) 
        {
            _agents.RemoveAll(a => a.Id == id);
            return RedirectToAction("Dashboard");
        }
    }
}