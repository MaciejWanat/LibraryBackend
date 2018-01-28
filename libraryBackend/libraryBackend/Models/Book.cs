using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Models
{
    public class Book
    {
        [Required]
        public Guid BookId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string Genre { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string EbookPath { get; set; } = @"/ebooks/default.mobi";
        [Required]
        [Column(TypeName = "Money")]
        public decimal Price { get; set; }

        public int Sold { get; set; }
    }
}
