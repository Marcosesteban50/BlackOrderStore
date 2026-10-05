using Black_order.Data;
using Black_order.DTOs.UserDTOs;
using Black_order.Services.UserServices;
using Google;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<UserController> logger;
        private readonly IUserService userService;

        public UserController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IConfiguration configuration,
            AppDbContext dbContext, IMapper mapper, ILogger<UserController> logger, IUserService userService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            this.userService = userService;
        }


        [HttpGet("UserList")]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserDTO>>> UserList()
        {

            var users = await userManager.Users.ToListAsync();

            var lista = new List<UserDTO>();

            foreach (var user in users)
            {

                var claims = await userManager.GetClaimsAsync(user);
                var roles = claims.Select(c => $"{c.Type}: {c.Value}").ToList();

                lista.Add(new UserDTO
                {
                    Email = user.Email!,
                    Roles = roles

                });
            }


            return lista;

        }


        // POST: api/User/Sign-Up
        [AllowAnonymous]
        [HttpPost("Sign-Up")]
        public async Task<ActionResult<AuthResponseDTO>> SignUp(UserCredentialsDTO userCredentialsDTO)
        {


            // Verify if the user already exists
            var user = new IdentityUser
            {
                Email = userCredentialsDTO.Email,
                UserName = userCredentialsDTO.Email
            };

            // Check if the user already exists
            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

            // If the user creation failed, return a BadRequest with the errors
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            //var perfil = new UserProfile
            //{
            //    UserId = user.Id,
            //    Email = user.Email,
            //    Address = "", // o null
            //    Phone = null,
            //    FullName = null
            //};


            // Add the user profile to the database
            //await dbContext.PerfilesUsuarios.AddAsync(perfil);

            //Save changes to the database
            await dbContext.SaveChangesAsync();

            // Add a default claim to the user (optional)
            await CustomerMaker(user);

            // Generate a token for the newly created user
            return await TokenBuilder(user);
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(UserCredentialsDTO userCredentialsDTO)
        {
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user == null)
            {
                return BadRequest(new { mensaje = "Error in login" });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password, lockoutOnFailure: false);


            if (!result.Succeeded)
            {

                return BadRequest(new { mensaje = "Error in login" });
            }

            //var perfil = await dbContext.PerfilesUsuarios.AnyAsync(x => x.UsuarioId == usuario.Id);

            //if (!perfil)
            //{
            //    var nuevoPerfil = new PerfilUsuario
            //    {
            //        UsuarioId = usuario.Id,
            //        Email = usuario.Email,
            //        DireccionEnvio = "", // o null
            //        Telefono = null,
            //        NombreCompleto = null
            //    };
            //    await dbContext.PerfilesUsuarios.AddAsync(nuevoPerfil);
            //    await dbContext.SaveChangesAsync();

            //}

            return await TokenBuilder(user);

        }


        //Token Builder Method
        private async Task<AuthResponseDTO> TokenBuilder(IdentityUser identityUser)
        {

            // Create claims for the token
            var claims = new List<Claim>
            {

                //Claim Id is used to store the user's unique identifier in the token
              new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
              //email = identityUser.Email
              //Claim email is used to store the user's email address in the token
              new Claim("email",identityUser.Email!),

            };

            // Get claims from the database for the user
            var claimsDB = await userManager.GetClaimsAsync(identityUser);


            // Add the claims from the database to the list of claims
            claims.AddRange(claimsDB);

            // Create a symmetric security key using the JWT key from the configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
                (configuration["jwtkey"]!));

            // Create signing credentials using the security key and HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set the token expiration to 1 year from now
            var expiration = DateTime.UtcNow.AddYears(1);

            // Create the JWT security token with the specified claims, expiration, and signing credentials
            var SecurityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiration, signingCredentials: creds);

            // Write the token to a string
            var token = new JwtSecurityTokenHandler().WriteToken(SecurityToken);

            // Return the AuthResponseDTO with the generated token and expiration date
            return new AuthResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }




        // Become Admin Method
        [HttpPost("MakeAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> HacerAdmin(EditClaimDTO editClaimDTO)
        {

            // Find the user by email using the UserManager
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            // If the user is not found, return a NotFound response with a message
            if (user == null)
            {
                return NotFound(new { mensaje = $"User not found {user}" });
            }

            // Check if the user already has the "Admin" claim
            var claims = await userManager.GetClaimsAsync(user);

            // If the user already has the "Admin" claim, return a BadRequest response with a message
            var isAdmin = claims.Any(x => x.Type == "Admin");

            if (isAdmin)
            {
                return BadRequest(new { mensaje = "The user is already an admin" });
            }

            // Add the "Admin" claim to the user if it doesn't already exist
            await userManager.AddClaimAsync(user, new Claim("Admin", "true"));


            // Return a NoContent response indicating that the operation was successful
            return NoContent();

        }


        // Method to add the "Customer" claim to the user if it doesn't already exist
        private async Task<bool> CustomerMaker(IdentityUser identityUser)
        {

            // Find the user by email using the identityUser's email
            var user = await userManager.FindByEmailAsync(identityUser.Email!);

            // If the user is not found, throw an exception
            if (user == null)
            {
                throw new Exception("Not Found");
            }


            // Check if the "Customer" claim already exists for the user
            var AlreadyCustomer = (await userManager.GetClaimsAsync(user)).Any(c => c.Type == "Customer");

            // If the "Customer" claim doesn't exist, add it to the user
            if (!AlreadyCustomer)
            {
                // Add the "Customer" claim to the user if it doesn't already exist
                await userManager.AddClaimAsync(user, new Claim("client", "true"));
            }

            return true;

        }
    }
}
