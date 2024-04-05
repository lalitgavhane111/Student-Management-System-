using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StudentManagement.Data;
using StudentManagement.Models;
using StudentManagement.ViewModel;

namespace StudentManagement.Controllers
{
    public class InstructorsController : Controller
    {

        private readonly StudentDbContext _context;

        public InstructorsController(StudentDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> InstructorsList(int? id, int? courseID)


        {
            var viewModel = new InstructorViewModel();
            viewModel.Instructors = await _context.Instructors
                  .Include(i => i.OfficeAssignment)
                  .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                  .OrderBy(i => i.LastName)
                  .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single();
                viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }

     


    public async Task<IActionResult> EditInstructor(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            var officeLocations = await _context.OfficeAssignments
                .Select(o => o.Location)
                .Distinct()
                .ToListAsync();

            ViewBag.Location = new SelectList(officeLocations); // Ensure ViewBag is correctly populated

            var viewModel = new InstructorViewModel
            {
                ID = instructor.ID,
                FirstMidName = instructor.FirstMidName,
                LastName = instructor.LastName,
                HireDate = instructor.HireDate,
                OfficeLocation = instructor.OfficeAssignment?.Location,
                Courses = await _context.Courses.ToListAsync(),
                SelectedCourses = instructor.CourseAssignments.Select(ca => ca.CourseID).ToList()
            };

            return View(viewModel);
        }


        [HttpPost, ActionName("EditInstructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstructorPost(int id, InstructorViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return NotFound();
            }

            if (id == viewModel.ID)
            {
                var instructorToUpdate = await _context.Instructors
                    .Include(i => i.OfficeAssignment)
                     .Include(i => i.CourseAssignments)
                    .FirstOrDefaultAsync(m => m.ID == id);

                if (instructorToUpdate == null)
                {
                    return NotFound();
                }

                instructorToUpdate.FirstMidName = viewModel.FirstMidName;
                instructorToUpdate.LastName = viewModel.LastName;
                instructorToUpdate.HireDate = viewModel.HireDate;

                if (instructorToUpdate.OfficeAssignment == null)
                {
                    instructorToUpdate.OfficeAssignment = new OfficeAssignment();
                }
                instructorToUpdate.OfficeAssignment.Location = viewModel.OfficeLocation;

                // Update courses taught by the instructor based on selected checkboxes
                instructorToUpdate.CourseAssignments.Clear();
                foreach (var courseId in viewModel.SelectedCourses)
                {
                    instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = id, CourseID = courseId });
                }

                try
                {
                    _context.Update(instructorToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(InstructorsList));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(viewModel.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Repopulate the dropdown list if model state is not valid
            var officeLocations = await _context.OfficeAssignments
                .Select(o => o.Location)
                .Distinct()
                .ToListAsync();

            ViewBag.Location = new SelectList(officeLocations, viewModel.OfficeLocation); // Pass selected value

            return View(viewModel);
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.ID == id);
        }


        //       // **CSRF attacks prevention  **  [ValidateAntiForgeryToken]//

        //           The **[ValidateAntiForgeryToken]** attribute is used to prevent Cross-Site Request Forgery(CSRF) attacks.
        //           CSRF attacks occur when a malicious website tricks a user's browser into performing actions on another
        //           website where the user is authenticated.



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            Instructor instructor = await _context.Instructors.Include(i => i.CourseAssignments).SingleAsync(i => i.ID == id);
            var department = await _context.Departments.Where(d => d.InstructorID == id).ToListAsync();
            department.ForEach(d => d.InstructorID = null);
            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction("InstructorsList");
        }







    }
}
