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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace libraryBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/rentals")]
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly LibraryContext _context;
        private readonly UserManager<LibraryUser> _userManager;

        public RentalsController(LibraryContext context, UserManager<LibraryUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Rentals
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IEnumerable<Rental>> GetRentals()
        {
            return await _context.Rentals.ToListAsync();
        }

        // GET: api/Rentals/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRental([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rental = await _context.Rentals.SingleOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            return Ok(rental);
        }

        // PUT: api/Rentals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRental([FromRoute] Guid id, [FromBody] Rental rental)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var error = await ValidateRental(rental);
            if (error != "")
            {
                return BadRequest(ReturnError(error));
            }

            if (id != rental.RentalId)
            {
                return BadRequest();
            }

            _context.Entry(rental).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RentalExistsId(id))
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

        // POST: api/Rentals
        [HttpPost]
        public async Task<IActionResult> PostRental([FromBody] Rental rental)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var error = await ValidateRental(rental);
            if(error != "")
            {
                return BadRequest(ReturnError(error));
            }

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRental", new { id = rental.RentalId }, rental);
        }

        // DELETE: api/Rentals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRental([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rental = await _context.Rentals.SingleOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }

            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();

            return Ok(rental);
        }

        // GET: api/Rentals/aaa@o2.pl
        [HttpGet("{email}")]
        public async Task<IActionResult> GetRentalsForUser([FromRoute] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rentals = await _context.Rentals.Where(m => m.UserEmail == email).ToListAsync();
            
            if (rentals.Count() == 0)
            {
                return NotFound();
            }

            return Ok(rentals);
        }

        private async Task<string> ValidateRental(Rental rental)
        {
            if (RentalExists(rental))
            {
                return "Book already owned: " + await GetBookName(rental.BookId);
            }

            var currentUserEmail = User.Claims.Where(e => e.Type == "sub").SingleOrDefault().Value;
            var currentUserRoles = await _userManager.GetRolesAsync(await _userManager.FindByEmailAsync(currentUserEmail));

            if (!currentUserRoles.Contains("Admin"))
            {
                if (currentUserEmail != rental.UserEmail)
                {
                    return "This user cannot add book rental for another user";
                }
            }

            if (!BookExists(rental.BookId))
            {
                return "Book with this id doesn't exist";
            }

            if (!UserExists(rental.UserEmail))
            {
                return "User with this email doesn't exist";
            }

            return "";
        }

        private bool RentalExistsId(Guid id)
        {
            return _context.Rentals.Any(e => e.RentalId == id);
        }

        private bool BookExists(Guid bookId)
        {
            return _context.Books.Any(e => e.BookId == bookId);
        }

        private bool UserExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }

        private bool RentalExists(Rental rental)
        {
            return _context.Rentals.Any(e => e.UserEmail == rental.UserEmail && e.BookId == rental.BookId);
        }

        private async Task<string> GetBookName(Guid bookId)
        {
            var book = await _context.Books.SingleOrDefaultAsync(m => m.BookId == bookId);
            return book.Title;
        }

        private Dictionary<string, string> ReturnError(string errorMessage)
        {
            return new Dictionary<string, string>
            {
                { "error", errorMessage }
            };
        }
    }
}