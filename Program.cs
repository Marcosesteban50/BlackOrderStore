using Black_order.Data;
using Black_order.Repositories.Categories;
using Black_order.Repositories.Carts;
using Black_order.Repositories.Favorites;
using Black_order.Repositories.Guides;
using Black_order.Repositories.Products;
using Black_order.Repositories.Profiles;
using Black_order.Repositories.Users;
using Black_order.Services.Files;
using Black_order.Services.UserServices;
using Black_order.Utilities.Errors;
using Google;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Global error handling: every handled exception becomes an RFC 7807 ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();


// Swagger
builder.Services.AddSwaggerGen(opc =>
{
    opc.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Or Http, depending on your setup
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    // Use the new delegate-based overload
    opc.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            // Use OpenApiSecuritySchemeReference instead of OpenApiSecurityScheme
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>{}
        }
    });
});


builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();


//Dbcontext configuration
builder.Services.AddDbContext<AppDbContext>(o =>
{

    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// OutPutcache service configuration
builder.Services.AddOutputCache();


// Get the global Mapster configuration
var mapsterConfig = TypeAdapterConfig.GlobalSettings;

// Scan the assembly for Mapster mapping profiles
mapsterConfig.Scan(Assembly.GetExecutingAssembly());

// Register Mapster configuration in dependency injection
builder.Services.AddSingleton(mapsterConfig);

// Register the Mapster mapper service
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddTransient<iFileStorage,LocalFileStorage>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IGuideRepository, GuideRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
           Encoding.UTF8.GetBytes(builder.Configuration["jwtkey"]!)),
        ClockSkew = TimeSpan.Zero
    };

});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("Admin"));
    options.AddPolicy("User", policy => policy.RequireClaim("User"));
    options.AddPolicy("AdminOrUser", policy => policy.RequireClaim("Admin", "User"));
    options.AddPolicy("Seller", policy => policy.RequireClaim("Seller"));
    options.AddPolicy("Customer", policy => policy.RequireClaim("Customer"));
});



// CORS
var urlsAllowed = builder.Configuration["urlsAllowed"]!.Split(",");
builder.Services.AddCors(x =>
{
    x.AddDefaultPolicy(o =>
    {
        o.WithOrigins(urlsAllowed)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials(); // Necesario para Google
    });
});

builder.Services.AddHttpContextAccessor();



var app = builder.Build();

// Must run first so it wraps every downstream middleware and endpoint.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
    //app.MapScalarApiReference();

}

app.UseSwagger();

app.UseHttpsRedirection();
app.UseCors();

app.UseStaticFiles();



app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    await GuidesSeeder.SeedAsync(app.Services, app.Logger);
}

app.Run();
