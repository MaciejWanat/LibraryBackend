using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Models
{
    public class LibraryUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        override public string Email { get; set; }

        public void SetChanges(RegistrationViewModel model, UserManager<LibraryUser> userManager)
        {
            if (model.Email != null)
            {
                Email = model.Email;
                UserName = model.Email;
            }

            if (model.FirstName != null)
                FirstName = model.FirstName;

            if (model.LastName != null)
                LastName = model.LastName;

            if (model.Password != null)
                PasswordHash = userManager.PasswordHasher.HashPassword(this, model.Password);
        }
    }
}
