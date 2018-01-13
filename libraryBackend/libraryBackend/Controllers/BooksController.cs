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
        public IEnumerable<Book> GetBooks()
        {
            return _context.Books;
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

                string returnPath = Path.Combine("images", "booksImages", "stock.jpg");
                string path = Path.Combine(_env.WebRootPath, returnPath);

                if (isImageGiven)
                {
                    returnPath = Path.Combine("images", "booksImages", model.Image.FileName);
                    path = Path.Combine(_env.WebRootPath, returnPath);

                    if (System.IO.File.Exists(path))
                    {
                        return BadRequest("File with that name already exists.");
                    }
                }

                Book book = model.BookObject();

                book.ImagePath = "/" + returnPath.ToString().Replace(@"\", "/");
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                if (isImageGiven)
                {
                    using (var fs = new FileStream(path, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fs);
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

                var removePath = _env.WebRootPath.ToString() + book.ImagePath.ToString().Replace("/", @"\");

                if(System.IO.File.Exists(removePath) && !removePath.ToString().Contains("stock.jpg"))
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
        [Route("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upload(IFormFile image)
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

        private bool BookExists(Guid id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}