using StudentManagement.Models;

namespace StudentManagement.ViewModel
{
    public class CourseViewModel
    {

        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<Course> Courses { get; set; }

    }
}
