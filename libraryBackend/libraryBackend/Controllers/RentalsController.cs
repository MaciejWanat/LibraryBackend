using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using libraryBackend.Data;
using libraryBackend.Models;

namespace libraryBackend.Controllers
{
    [Produces("application/json")]
    [Route("api/rentals")]
    public class RentalsController : Controller
    {
        private readonly LibraryContext _context;

        public RentalsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/Rentals
        [HttpGet]
        public IEnumerable<Rental> GetRentals()
        {
            return _context.Rentals;
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
                if (!RentalExists(id))
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

        private bool RentalExists(Guid id)
        {
            return _context.Rentals.Any(e => e.RentalId == id);
        }
    }
}