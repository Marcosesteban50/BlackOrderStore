using Black_order.Data;
using Black_order.Repositories.Admin;
using Black_order.Repositories.Categories;
using Black_order.Repositories.Carts;
using Black_order.Repositories.Favorites;
using Black_order.Repositories.Guides;
using Black_order.Repositories.Orders;
using Black_order.Repositories.Products;
using Black_order.Repositories.Profiles;
using Black_order.Repositories.Reviews;
using Black_order.Repositories.Users;
using Black_order.Services.Files;
using Black_order.Services.Orders;
using Black_order.Services.Payments;
using Black_order.Services.UserServices;
using Black_order.Utilities.Errors;
using Google;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Console/debug logging only. The default Windows EventLog provider throws
// "Cannot open log for source '.NET Runtime'" when the process cannot write to
// the Event Log, and LoggerFactory rethrows provider failures, taking the whole
// API down on the first logged error (for example, a database connection issue).
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
// SQL Server is the production provider; SQLite is available for local/test runs
// via Database:Provider=Sqlite (no SQL Server instance required).
var databaseProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        o.UseSqlite(connectionString);
        return;
    }

    o.UseSqlServer(connectionString);
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
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Payments: Stripe only when a secret key is configured, otherwise manual flow.
if (!string.IsNullOrWhiteSpace(builder.Configuration["Stripe:SecretKey"]))
{
    builder.Services.AddScoped<IPaymentService, StripePaymentService>();
}
else
{
    builder.Services.AddScoped<IPaymentService, ManualPaymentService>();
}


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
    options.AddPolicy("SellerOrAdmin", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(claim => claim.Type == "Seller") ||
        context.User.HasClaim(claim => claim.Type == "Admin")));
    options.AddPolicy("Customer", policy => policy.RequireClaim("Customer"));
});

// Brute-force protection on the auth endpoints.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
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
app.UseRateLimiter();

app.UseStaticFiles();



app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.MapControllers();

if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
{
    // SQLite has no SQL Server migrations; create the schema from the model.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    await DevAdminSeeder.SeedAsync(app.Services, builder.Configuration, app.Logger);

    // Seed after the server is listening: a slow or unavailable database must not
    // delay startup. The seeder catches and logs its own errors.
    app.Lifetime.ApplicationStarted.Register(() =>
        _ = Task.Run(() => GuidesSeeder.SeedAsync(app.Services, app.Logger)));
}

app.Run();
