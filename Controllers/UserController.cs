using Black_order.DTOs.GoogleDTOs;
using Black_order.DTOs.UserDTOs;
using Black_order.Repositories.Profiles;
using Black_order.Repositories.Users;
using Black_order.Services.UserServices;
using Google;
using Google.Apis.Auth;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Black_order.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IProfileRepository profileRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly ILogger<UserController> logger;

        public UserController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IConfiguration configuration,
            IProfileRepository profileRepository, IUserRepository userRepository,
            IMapper mapper, ILogger<UserController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.profileRepository = profileRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        // GET: api/User/UserList
        [HttpGet("UserList")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> UserList()
        {
            var users = await userRepository.GetUsersWithClaimsAsync();
            return Ok(users);
        }

        // POST: api/User/Sign-Up
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("Sign-Up")]
        public async Task<ActionResult<AuthResponseDTO>> SignUp(UserCredentialsDTO userCredentialsDTO)
        {
            // Initialize new identity user object
            var user = new IdentityUser
            {
                Email = userCredentialsDTO.Email,
                UserName = userCredentialsDTO.NickName,
                NormalizedEmail = userCredentialsDTO.Email.ToUpper(),
                NormalizedUserName = userCredentialsDTO.NickName.ToUpper()
            };

            // Check if the user already exists in the database
            var userExists = await userManager.FindByEmailAsync(user.Email!);
            if (userExists != null)
            {
                return BadRequest(new { mensaje = "Error in sign up" });
            }

            // Create user with provided password
            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Ensure the user profile exists
            await EnsureProfileAsync(user);

            await profileRepository.SaveChangesAsync();

            // Assign default customer claim
            await CustomerMaker(user);

            // Generate and return JWT token
            return await TokenBuilder(user);
        }

        // POST: api/User/Login
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(UserLoginDTO userCredentialsDTO)
        {
            // Find the user by email first, then by username (nickname).
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email)
                ?? await userManager.FindByNameAsync(userCredentialsDTO.Email);
            if (user == null)
            {
                return BadRequest(new { mensaje = "Error in login" });
            }

            // Validate user credentials
            var result = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return BadRequest(new { mensaje = "Error in login" });
            }

            // Ensure profile exists for existing users
            await EnsureProfileAsync(user);

            await profileRepository.SaveChangesAsync();

            // Generate and return JWT token
            return await TokenBuilder(user);
        }

        // Token Builder Method
        private async Task<AuthResponseDTO> TokenBuilder(IdentityUser identityUser)
        {
            // Define core claims for the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
                new Claim("email", identityUser.Email!),
            };

            // Retrieve additional claims from the database
            var claimsDB = await userManager.GetClaimsAsync(identityUser);
            claims.AddRange(claimsDB);

            // Configure security key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set token expiration to 60 minutes
            var expiration = DateTime.UtcNow.AddMinutes(60);

            // Build the JWT token
            var SecurityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiration, signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(SecurityToken);

            return new AuthResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }

        // POST: api/User/MakeAdmin
        [HttpPost("MakeAdmin")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> MakeAdmin(EditClaimDTO editClaimDTO)
        {
            // Find user target for promotion
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);
            if (user == null)
            {
                return NotFound(new { mensaje = $"User not found {editClaimDTO.Email}" });
            }

            // Verify if user already possesses admin privileges
            var claims = await userManager.GetClaimsAsync(user);
            var isAdmin = claims.Any(x => x.Type == CustomClaims.Admin);

            if (isAdmin)
            {
                return BadRequest(new { mensaje = "The user is already an admin" });
            }

            // Grant Admin claim
            await userManager.AddClaimAsync(user, new Claim(CustomClaims.Admin, "true"));

            return NoContent();
        }

        // Helper method to assign default Customer claim
        private async Task<bool> CustomerMaker(IdentityUser identityUser)
        {
            var user = await userManager.FindByEmailAsync(identityUser.Email!);
            if (user == null)
            {
                throw new Exception("Not Found");
            }

            var AlreadyCustomer = (await userManager.GetClaimsAsync(user)).Any(c => c.Type == CustomClaims.Customer);

            if (!AlreadyCustomer)
            {
                await userManager.AddClaimAsync(user, new Claim(CustomClaims.Customer, "true"));
            }

            return true;
        }

        // POST: api/User/google-login
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("google-login")]
        public async Task<ActionResult<CompleteAuthResponseDTO>> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                // Validate Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.Token,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [configuration["GoogleClientId"]!]
                    });

                var user = await userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    // Register user if they do not exist
                    user = new IdentityUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    await userManager.AddClaimAsync(user, new Claim(CustomClaims.GoogleId, payload.Subject));
                    await CustomerMaker(user);
                    await EnsureProfileAsync(user);
                }
                else
                {
                    // Link GoogleId if not already present
                    var existingClaims = await userManager.GetClaimsAsync(user);
                    if (!existingClaims.Any(c => c.Type == CustomClaims.GoogleId))
                    {
                        await userManager.AddClaimAsync(user, new Claim(CustomClaims.GoogleId, payload.Subject));
                    }
                }

                await EnsureProfileAsync(user);

                // Build token and gather roles
                var authResponse = await TokenBuilder(user);
                var claims = await userManager.GetClaimsAsync(user);

                var roles = claims.Where(c => c.Type == CustomClaims.Admin || c.Type == CustomClaims.Customer || c.Type == CustomClaims.Seller)
                                 .Select(c => $"{c.Type}: {c.Value}")
                                 .ToList();

                await profileRepository.SaveChangesAsync();

                return new CompleteAuthResponseDTO
                {
                    Token = authResponse.Token,
                    Expiration = authResponse.Expiration,
                    Usuario = new UserInfoDTO
                    {
                        Email = user.Email!,
                        Name = payload.Name ?? user.Email!.Split('@')[0],
                        Picture = payload.Picture,
                        GoogleId = payload.Subject
                    }
                };
            }
            catch (InvalidJwtException ex)
            {
                logger.LogWarning("Invalid Google Token: {Message}", ex.Message);
                return Unauthorized(new { mensaje = "Invalid Google Token" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Internal Server Error");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // Centralized custom claims constants
        public static class CustomClaims
        {
            public const string Admin = "Admin";
            public const string Seller = "Seller";
            public const string Customer = "Customer";
            public const string GoogleId = "GoogleId";
        }

        // Ensures user profile exists in database
        private async Task EnsureProfileAsync(IdentityUser user)
        {
            var exists = await profileRepository.GetByUserIdAsync(user.Id);
            if (exists == null)
            {
                var perfil = new UserProfile
                {
                    UserId = user.Id,
                    Email = user.Email,
                    NickName = user.UserName,
                    Address = "",
                    Phone = null,
                    FullName = null
                };
                await profileRepository.AddAsync(perfil);
                await profileRepository.SaveChangesAsync();
            }
        }


        [HttpGet("whoami")]
        [Authorize]  // sin policy, solo autenticado
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }


        [HttpGet("test-admin")]
        [Authorize]
        public IActionResult TestAdmin()
        {
            // Comprobamos si el usuario tiene el claim Admin.
            var isAdmin = User.HasClaim("Admin", "true");

            // Mostramos el resultado de la comprobación.
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                IsAdmin = isAdmin,
                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }
    }
}