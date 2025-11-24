namespace Classly.Models.AIGen
{
    public class ClassNotesViewModel
    {
        public string RichText { get; set; }
        public IFormFile FileUpload { get; set; }
        public string Difficulty { get; set; }
        public Guid StudentId { get; set; }

    }

}
