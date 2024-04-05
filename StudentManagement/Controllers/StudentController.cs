
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {

        private readonly StudentDbContext _context;

        public StudentController(StudentDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetAllStudents()
        {
            return View(await _context.Students.ToListAsync());
        }

        public async Task<IActionResult> StudentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        [HttpGet]
        public async Task<IActionResult> NewStudent ()
        {
           
            return View ();     
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewStudent([Bind("EnrollmentDate,FirstMidName,LastName")] Student student)
        {
          
            if (student.ID == 0)
            {

                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("GetAllStudents");
            }
            return View(GetAllStudents);
        }
    }
}
