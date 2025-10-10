using Classly.Models.Courses;
using Classly.Models.Tutors;

namespace Classly.Dummy.Datasets
{
    public static class CoursesDummyDataset
    {
        private static List<Course> _courses = new List<Course>
            {
                new Course
                {
                    Id = Guid.NewGuid(),
                    Name = "Introduction to Philosophy",
                    Description = "Explore the fundamental questions of existence and knowledge.",
                    Tutor = new Tutor
                    {
                        Id = Guid.NewGuid(),
                        Firstname = "Alice",
                        Lastname = "Carter",
                        Email = "alice.carter@example.com"
                    },
                    DateCommencing = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 30),
                    Price = 10.00m
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Name = "Advanced Mathematics",
                    Description = "Dive deep into calculus and linear algebra.",
                    Tutor = new Tutor
                    {
                        Id = Guid.NewGuid(),
                        Firstname = "Brian",
                        Lastname = "Lee",
                        Email = "brian.lee@example.com"
                    },
                    DateCommencing = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
                    StartTime = new TimeOnly(11, 0),
                    EndTime = new TimeOnly(12, 45),
                    Price = 10.00m
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Name = "Modern Art History",
                    Description = "Study the evolution of art in the 20th century.",
                    Tutor = new Tutor
                    {
                        Id = Guid.NewGuid(),
                        Firstname = "Clara",
                        Lastname = "Nguyen",
                        Email = "test@mail.com"
                    },
                    DateCommencing = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
                    StartTime = new TimeOnly(11, 0),
                    EndTime = new TimeOnly(12, 45),
                    Price = 10.00m
                }
            };
        public static List<Course> GetCourses()
        {
            return _courses;
        }
    }
}
