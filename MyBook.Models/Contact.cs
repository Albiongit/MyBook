using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Models
{
    public class Contact
    {
        [Required]
        public string Email { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public int PhoneNumber { get; set; }

        public string Message { get; set; }
    }
}
