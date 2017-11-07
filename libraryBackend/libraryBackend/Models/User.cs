using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Rental> Rentals { get; set; }
    }
}
