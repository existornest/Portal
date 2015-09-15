using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.ViewModels
{
    public class Register
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Login")]
        [Required(ErrorMessage = "Login jest wymagany")]
        public string UserName { get; set; }

        [Display(Name = "Adres email")]
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Niewłaściwy adres email")]
        public string Email { get; set; }

        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(255, ErrorMessage = "Długość hasła musi być pomiędzy 5 a 255 znaków", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Potwierdź hasło")]
        [Required(ErrorMessage = "Hasła są różne.")]
        [StringLength(255, ErrorMessage = "Długość hasła musi być pomiędzy 5 a 255 znaków", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }
}