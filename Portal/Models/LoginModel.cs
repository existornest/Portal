using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class LoginModel
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Login")]
        [Required(ErrorMessage = "Login jest wymagany")]
        public string UserName { get; set; }

        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(255, ErrorMessage = "Długość hasła musi być pomiędzy 5 a 255 znaków", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}