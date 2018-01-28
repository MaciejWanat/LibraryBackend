using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace libraryBackend.Models.ViewModels
{
    public class BookUploadViewModel : Book
    {
        public IFormFile Image { get; set; }
        public IFormFile Ebook { get; set; }

        public Book BookObject()
        {
            return new Book
            {
                Author = Author,
                BookId = BookId,
                Description = Description,
                Genre = Genre,
                ImagePath = ImagePath,
                EbookPath = EbookPath,
                Price = Price,
                Sold = Sold,
                Title = Title
            };
        }
    }
}
