using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Models
{
    public class Rental
    {
        public Guid RentalId { get; set; }

        public Book Book { get; set; }
        public User User { get; set; }        
    }
}
