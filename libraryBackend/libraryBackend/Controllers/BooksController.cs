using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using libraryBackend.Data;
using libraryBackend.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using libraryBackend.Models.ViewModels;
using System.Diagnostics;

namespace libraryBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/books")]
    [Authorize]
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;
        private readonly IHostingEnvironment _env;

        public BooksController(LibraryContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }
        
        // GET: api/Books
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // GET: api/Books/Bestsellers 
        [HttpGet("bestsellers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBestsellers()
        {            
            if (ModelState.IsValid)
            {
                var bestsellers = await _context.Books.Where(m => m.Sold > 10).OrderByDescending(m => m.Sold).ToListAsync();

                if (bestsellers == null)
                {
                    return NotFound();
                }

                return Ok(bestsellers);
            }

            return BadRequest(ModelState);
        }

        // GET: api/Books/Bestsellers/genre 
        [HttpGet("bestsellers/{genre}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBestsellersByGenre([FromRoute] string genre)
        {            
            if (ModelState.IsValid)
            {
                var bestsellers = await _context.Books.Where(m => m.Sold > 10 && m.Genre == genre).OrderByDescending(m => m.Sold).ToListAsync();

                if (bestsellers == null)
                {
                    return NotFound();
                }

                return Ok(bestsellers);
            }

            return BadRequest(ModelState);
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBook([FromRoute] Guid id)
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == id);

                if (book == null)
                {
                    return NotFound();
                }

                return Ok(book);
            }

            return BadRequest(ModelState);
        }
        
        // PUT: api/Books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBook([FromRoute] Guid id, [FromBody] Book book)
        {
            if (ModelState.IsValid)
            {
                if (id != book.BookId)
                {
                    return BadRequest();
                }

                _context.Entry(book).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            
            return BadRequest(ModelState);
        }

        // POST: api/Books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostBook(BookUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isImageGiven = model.Image != null;
                bool isEbookGiven = model.Ebook != null;

                string returnPath = Path.Combine("images", "booksImages", "stock.jpg");
                string path = Path.Combine(_env.WebRootPath, returnPath);

                string returnPathEbook = Path.Combine("ebooks", "default.mobi");
                string pathEbook = Path.Combine(_env.WebRootPath, returnPathEbook);

                if(isEbookGiven)
                {
                    returnPathEbook = Path.Combine("ebooks", model.Ebook.FileName);
                    pathEbook = Path.Combine(_env.WebRootPath, returnPathEbook);

                    if (System.IO.File.Exists(pathEbook))
                    {
                        return BadRequest(ReturnError("Ebook: File with that name already exists."));
                    }
                }

                if (isImageGiven)
                {
                    returnPath = Path.Combine("images", "booksImages", model.Image.FileName);
                    path = Path.Combine(_env.WebRootPath, returnPath);

                    if (System.IO.File.Exists(path))
                    {
                        return BadRequest(ReturnError("Image: File with that name already exists."));
                    }
                }

                Book book = model.BookObject();

                book.ImagePath = "/" + returnPath.ToString().Replace(@"\", "/");
                book.EbookPath = "/" + returnPathEbook.ToString().Replace(@"\", "/");
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                if (isImageGiven)
                {
                    using (var fs = new FileStream(path, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fs);
                    }
                }

                if (isEbookGiven)
                {
                    using (var fs = new FileStream(pathEbook, FileMode.Create))
                    {
                        await model.Ebook.CopyToAsync(fs);
                    }
                }

                return CreatedAtAction("GetBook", new { id = book.BookId }, book);
            }

            return BadRequest(ModelState);            
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook([FromRoute] Guid id)
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    return NotFound();
                }
                
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                await DeleteRentalsForBook(book.BookId);

                var removePath = _env.WebRootPath.ToString() + book.ImagePath.ToString().Replace("/", @"\");

                if(System.IO.File.Exists(removePath) && !removePath.ToString().Contains("stock.jpg"))
                {
                    System.IO.File.Delete(removePath);
                }

                removePath = _env.WebRootPath.ToString() + book.EbookPath.ToString().Replace("/", @"\");

                if (System.IO.File.Exists(removePath) && !removePath.ToString().Contains("default.mobi"))
                {
                    System.IO.File.Delete(removePath);
                }

                return Ok(book);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("genre/{genre}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByGenre([FromRoute] string genre)
        {
            if (ModelState.IsValid)
            {                  
                var book = await _context.Books.Where(m => m.Genre == genre).ToListAsync();

                if (book == null)
                {
                    return NotFound();
                }

                return Ok(book);
            }

            return BadRequest(ModelState);
        }

        [HttpGet("genre")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGenres()
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.Select(m => m.Genre).Distinct().ToListAsync();

                if (book == null)
                {
                    return NotFound();
                }

                return Ok(book);
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("upload/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image.Length > 0)
            {
                string returnPath = Path.Combine("images", "booksImages", image.FileName);
                string path = Path.Combine(_env.WebRootPath, returnPath);

                using (var fs = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }

                return Ok(returnPath);
            }
            return BadRequest(image);
        }

        [HttpGet("page/{pageSize}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPages([FromRoute] int pageSize)
        {
            string genre = HttpContext.Request.Query["genre"].ToString();

            if (String.IsNullOrEmpty(genre))
                return Ok(await GetPagesAmount(pageSize));

            return Ok(await GetPagesAmount(pageSize, genre));            
        }
        
        [HttpGet("page/{pageSize}/{pageNum}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPage([FromRoute] int pageSize, [FromRoute] int pageNum)
        {
            string genre = HttpContext.Request.Query["genre"].ToString();
            bool emptyGenre = String.IsNullOrEmpty(genre);
            int pagesAmount;

            if (emptyGenre)
                pagesAmount = await GetPagesAmount(pageSize);            
            else
                pagesAmount = await GetPagesAmount(pageSize, genre);

            if (1 <= pageNum && pageNum <= pagesAmount)
            {
                var ToSkip = (pageNum - 1) * pageSize;

                if (emptyGenre)
                    return Ok(await _context.Books.Skip(ToSkip).Take(pageSize).ToListAsync());
                else
                    return Ok(await _context.Books.Where(e => e.Genre == genre).Skip(ToSkip).Take(pageSize).ToListAsync());                
            }
            else
            {
                return BadRequest();
            }
        }

        // DELETE: api/Books/genre/{genre}
        [HttpDelete("genre/{genre}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre([FromRoute] string genre)
        {
            var books = await _context.Books.Where(e => e.Genre == genre).ToListAsync();

            if (!books.Any())
            {
                return NotFound();
            }

            _context.Books.RemoveRange(books);
            await _context.SaveChangesAsync();

            foreach (var book in books)
            {
                var removePath = _env.WebRootPath.ToString() + book.ImagePath.ToString().Replace("/", @"\");
                await DeleteRentalsForBook(book.BookId);

                if (System.IO.File.Exists(removePath) && !removePath.ToString().Contains("stock.jpg"))
                {
                    System.IO.File.Delete(removePath);
                }
                
                removePath = _env.WebRootPath.ToString() + book.EbookPath.ToString().Replace("/", @"\");

                if (System.IO.File.Exists(removePath) && !removePath.ToString().Contains("default.mobi"))
                {
                    System.IO.File.Delete(removePath);
                }
            }

            return Ok(books);           
        }

        [HttpPost]
        [Route("upload/ebook")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadEbook(Guid bookId, IFormFile ebook)
        {
            if (ebook.Length > 0)
            {
                string returnPath = Path.Combine("ebooks", ebook.FileName);
                string path = Path.Combine(_env.WebRootPath, returnPath);

                if (System.IO.File.Exists(path))
                {
                    return BadRequest(ReturnError("File with that name already exists."));
                }

                if (!BookExists(bookId))
                {
                    return BadRequest(ReturnError("Book with this id doesn't exist."));
                }

                var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == bookId);

                if (book.EbookPath != @"/ebooks/default.mobi")
                {
                    return BadRequest(ReturnError("This book already has non-default ebook assigned."));
                }

                book.EbookPath = "/" + returnPath.ToString().Replace(@"\", @"/");

                _context.Entry(book).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                using (var fs = new FileStream(path, FileMode.Create))
                {
                    await ebook.CopyToAsync(fs);
                }

                return Ok(book);
            }

            return BadRequest(ebook);
        }

        [HttpDelete]
        [Route("upload/ebook")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEbook(Guid bookId)
        {
            if (!BookExists(bookId))
            {
                return BadRequest(ReturnError("Book with this id doesn't exist."));
            }

            var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == bookId);

            if (book.EbookPath == @"/ebooks/default.mobi")
            {
                return BadRequest(ReturnError("This book already has default ebook assigned."));
            }

            var path = _env.WebRootPath.ToString() + book.EbookPath.ToString().Replace("/", @"\");

            if (!System.IO.File.Exists(path))
            {
                return BadRequest(ReturnError("File with that name doesn't exist."));
            }
            
            book.EbookPath = @"/ebooks/default.mobi";

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            System.IO.File.Delete(path);

            return Ok(book);
        }

        [HttpGet("ebook/{bookId}")]
        public async Task<IActionResult> GetEbook([FromRoute] Guid bookId)
        {
            if (!BookExists(bookId))
            {
                return BadRequest(ReturnError("Book with this id doesn't exist."));
            }

            var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == bookId);
            var path = _env.WebRootPath.ToString() + book.EbookPath.ToString().Replace("/", @"\");

            if (!System.IO.File.Exists(path))
            {
                return BadRequest(ReturnError("File with that name doesn't exist."));
            }

            var stream = new FileStream(path, FileMode.Open);

            return File(stream, "application/octet-stream");
        }

        private async Task<int> GetPagesAmount(int pageSize, string genre = "default")
        {
            int BooksAmount; 

            if (genre == "default")            
                BooksAmount = await _context.Books.CountAsync();
            else
                BooksAmount = await _context.Books.Where(e => e.Genre == genre).CountAsync();

            return (int)Math.Ceiling(BooksAmount / (double)pageSize);
        }

        private async Task DeleteRentalsForBook(Guid bookId)
        {
            var rentals = await _context.Rentals.Where(m => m.BookId == bookId).ToListAsync();
            if (rentals.Any())
            {
                _context.Rentals.RemoveRange(rentals);
                await _context.SaveChangesAsync();
            }
        }

        private Dictionary<string,string> ReturnError(string errorMessage)
        {
            return new Dictionary<string, string>
            {
                { "error", errorMessage }
            };
        }

        private bool BookExists(Guid id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}