using StudentManagement.Models;

namespace StudentManagement.ViewModel
{


        public class InstructorViewModel
        {
            public IEnumerable<Instructor> Instructors { get; set; }
            public IEnumerable<Course> Courses { get; set; }
            public IEnumerable<Enrollment> Enrollments { get; set; }

        // Properties for editing instructor details
        public int ID { get; set; }
        public string FirstMidName { get; set; }
        public string LastName { get; set; }
        public DateTime HireDate { get; set; }
        public string OfficeLocation { get; set; }

   
        public List<int> SelectedCourses { get; set; }  //   select cources using checkbox

    }
}
