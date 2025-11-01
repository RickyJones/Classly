using Classly.Models.Courses;

namespace Classly.Models.Booking
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Course Course { get; set; }
        public User BookedUser { get; set; }
        public DateTime BookedOnDate { get; set; }
     
    }
}
