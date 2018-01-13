using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using libraryBackend.Data;
using libraryBackend.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace libraryBackend.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly UserManager<LibraryUser> _userManager;
        private readonly SignInManager<LibraryUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<LibraryUser> userManager, SignInManager<LibraryUser> signInManager, IConfiguration Configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = Configuration;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IEnumerable<LibraryUser> GetUsers()
        {
            return _userManager.Users.ToList();
        }

        // PUT: api/Users/aaa@bbb.com
        [HttpPut("{email}")]
        public async Task<IActionResult> PutUser([FromRoute] string email, [FromBody] RegistrationViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }
            
            user.SetChanges(model, _userManager);

            await _userManager.UpdateSecurityStampAsync(user);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // POST: api/Users
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostUser([FromBody] RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new LibraryUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(await _userManager.FindByEmailAsync(user.Email), "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return await GenerateJwtToken(model.Email, user);
                }

                return BadRequest(result);
            }            

            return BadRequest();
        }

        // DELETE: api/Users/aaa@bbb.com
        [HttpDelete("{email}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound();
                }

                await _userManager.DeleteAsync(user);

                return Ok(user);
            }

            return BadRequest();
        }

        // GET: api/Users/aaa@bbb.pl
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail([FromRoute] string email)
        {
            if (ModelState.IsValid)
            {              
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            return BadRequest();
        }

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager
                    .PasswordSignInAsync(
                        model.Email,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var appUser = await _userManager.FindByEmailAsync(model.Email);
                    return await GenerateJwtToken(model.Email, appUser);
                }
            }            

            return BadRequest();           
        }
        
        // POST: api/users/logout  
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        private async Task<IActionResult> GenerateJwtToken(string email, LibraryUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}