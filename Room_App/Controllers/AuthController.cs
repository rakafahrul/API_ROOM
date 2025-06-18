using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Room_App.Data;
using Room_App.Models;
using Room_App.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(IUserService userService, IConfiguration configuration, ApplicationDbContext context)
        {
            _userService = userService;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("users")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                foreach (var user in users)
                {
                    user.Password = null;
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/me")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                    return Unauthorized("Invalid token");

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                var token = GenerateJwtToken(user);
                user.Password = null;

                return Ok(new LoginResponse
                {
                    Token = token,
                    Role = user.Role,
                    User = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterRequest request)
        {
            try
            {
                if (await _userService.EmailExistsAsync(request.Email))
                    return BadRequest("Email already in use");

                var user = new User
                {
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name,
                    Role = request.Role,
                    Photo = "",
                    CreatedAt = DateTime.Now
                };

                var createdUser = await _userService.CreateUserAsync(user);
                createdUser.Password = null;
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPut("user/me")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                    return Unauthorized("Invalid token");

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                user.Name = request.Name;
                user.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Photo))
                    user.Photo = request.Photo;
                if (!string.IsNullOrEmpty(request.Password))
                    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var success = await _userService.UpdateUserAsync(user);
                if (!success)
                    return BadRequest("Failed to update profile");

                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("user/upload_photo")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return BadRequest("No photo uploaded");

                var ext = Path.GetExtension(photo.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowed.Contains(ext))
                    return BadRequest("Only image files allowed (.jpg, .jpeg, .png, .gif)");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                    return Unauthorized("Invalid token");

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var photoUrl = $"{baseUrl}/uploads/{fileName}";
                user.Photo = photoUrl;
                await _userService.UpdateUserAsync(user);

                return Ok(new { photoUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Photo upload error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}









/*using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Room_App.Models;
using Room_App.Services;
using Room_App.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Room_App.Data;

namespace Room_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;


        public AuthController(IUserService userService, IConfiguration configuration, ApplicationDbContext context)
        {
            _userService = userService;
            _configuration = configuration;
            _context = context;

        }


        // Tambahkan endpoint ini ke AuthController.cs
        [HttpGet("users")]
        [Authorize(Roles = "admin")] // Hanya admin yang bisa mengakses
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                // Opsional: Hilangkan password dari respons untuk keamanan
                foreach (var user in users)
                {
                    user.Password = null;
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // Tambahkan endpoint ini di AuthController
        [HttpGet("user/me")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                // Jangan mengembalikan password
                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                // Log untuk debugging
                Console.WriteLine($"Login attempt for email: {request.Email}");

                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                {
                    Console.WriteLine("Authentication failed: Invalid email or password");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var token = GenerateJwtToken(user);
                Console.WriteLine($"User {user.Id} authenticated successfully");

                // Jangan mengembalikan password dalam respons
                user.Password = null;

                return Ok(new LoginResponse
                {
                    Token = token,
                    Role = user.Role,
                    User = user
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                if (await _userService.EmailExistsAsync(request.Email))
                    return BadRequest("Email already in use");

                var user = new User
                {
                    Email = request.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name,
                    Role = request.Role,
                    Photo = "",
                    CreatedAt = DateTime.Now
                };

                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                // Don't return password
                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private string GenerateJwtToken(User user)
        {
            // Implementasi generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPut("user/me")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                // Update user properties
                user.Name = request.Name;
                user.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Photo))
                {
                    user.Photo = request.Photo;
                }

                var success = await _userService.UpdateUserAsync(user);
                if (!success)
                    return BadRequest("Failed to update profile");

                // Jangan mengembalikan password
                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("user/upload_photo")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return BadRequest("No photo uploaded");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                    return Unauthorized("Invalid token");

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var photoUrl = $"{baseUrl}/uploads/{fileName}";
                user.Photo = photoUrl;
                await _userService.UpdateUserAsync(user);

                return Ok(new { photoUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("user/me")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                user.Name = request.Name;
                user.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Photo))
                {
                    user.Photo = request.Photo;
                }
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                var success = await _userService.UpdateUserAsync(user);
                if (!success)
                    return BadRequest("Failed to update profile");

                user.Password = null;
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}*/