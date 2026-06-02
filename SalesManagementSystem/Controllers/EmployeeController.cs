using System;
using System.Web.Mvc;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Repositories;

namespace SalesManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository _employeeRepo;

        public EmployeeController(EmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        // GET: Employee
        public ActionResult Index()
        {
            var employees = _employeeRepo.GetAll();
            return View(employees);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            return View(new Employee());
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Optionally set NguoiTao from user session/identity here
                employee.NguoiTao = 0; // Default save as 0
                _employeeRepo.Insert(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        // GET: Employee/Update/5
        public ActionResult Update(int id)
        {
            var employee = _employeeRepo.GetById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employee/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.NguoiCapNhat = 0; // Default save as 0
                _employeeRepo.Update(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            _employeeRepo.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
