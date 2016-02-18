// model for contact form page

using System.ComponentModel.DataAnnotations;

namespace WebRole1.Models
{
    public class ContactForm
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
