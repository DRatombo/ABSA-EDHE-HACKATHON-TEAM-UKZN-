using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using VERA.Business.Services;
using VERA.Data.Context;
using VERA.Registry.Data;

var builder = WebApplication.CreateBuilder(args);


// Add MVC so we can use controllers and views
builder.Services.AddControllersWithViews();


// MAIN VERA DATABASE

// Connect to the main VERA database
builder.Services.AddDbContext<VeraDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// REGISTRY DATABASE

// Connect to the separate Registry database
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RegistryConnection")));


// VERA BUSINESS SERVICES

// Register the services used to assess an SME opportunity
builder.Services.AddScoped<FundingCalculatorService>();
builder.Services.AddScoped<FingerprintService>();
builder.Services.AddScoped<DuplicateDetectionService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<FulfilmentAssessmentService>();
builder.Services.AddScoped<OpportunityDecisionService>();
builder.Services.AddScoped<FulfilmentPassportService>();

// Runs the full opportunity assessment process
builder.Services.AddScoped<OpportunityAssessmentService>();


// REGISTRY SERVICES

// These services are used to check and verify purchase orders
builder.Services.AddScoped<VERA.Registry.Services.DocumentHashService>();
builder.Services.AddScoped<VERA.Registry.Services.FinancingClaimService>();

// Full name is used because VERA.Business also has a FingerprintService
builder.Services.AddScoped<VERA.Registry.Services.FingerprintService>();

builder.Services.AddScoped<VERA.Registry.Services.PdfDocumentAnalysisService>();
builder.Services.AddScoped<VERA.Registry.Services.RegistryVerificationService>();
builder.Services.AddScoped<VERA.Registry.Services.VeraIdService>();


// RATE LIMITING

// Limits repeated requests from the same IP address
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            // Use the user's IP address to create a request limit
            var clientIp =
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                clientIp,
                _ => new FixedWindowRateLimiterOptions
                {
                    // Allow a reasonable number of requests per minute
                    PermitLimit = 100,

                    // Start a fresh limit every minute
                    Window = TimeSpan.FromMinutes(1),

                    // Do not queue large numbers of extra requests
                    QueueLimit = 0,

                    AutoReplenishment = true
                });
        });
});


// BUILD APP

var app = builder.Build();


// ERROR HANDLING

// Show a normal error page instead of developer errors when deployed
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Tell browsers to only use HTTPS for the site
    app.UseHsts();
}


// HTTPS

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();


// SECURITY HEADERS

// Add browser security headers to every response
app.Use(async (context, next) =>
{
    // Stops browsers from guessing a different content type
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Stops VERA from being loaded inside another website's frame
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Reduces the amount of referrer information shared with other sites
    context.Response.Headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    // VERA does not need access to these browser/device features
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    // Prevent older browsers from caching sensitive authenticated pages
    if (context.Request.Path.StartsWithSegments("/SME") ||
        context.Request.Path.StartsWithSegments("/Registry") ||
        context.Request.Path.StartsWithSegments("/Assessment"))
    {
        context.Response.Headers["Cache-Control"] =
            "no-store, no-cache, must-revalidate";

        context.Response.Headers["Pragma"] = "no-cache";
    }

    await next();
});


// STATIC FILES

// Allow the website to use CSS, JavaScript and images from wwwroot
app.UseStaticFiles();


// ROUTING

// Enable routing between pages and controllers
app.UseRouting();


// RATE LIMITING

// Apply request limits after static files have been handled
app.UseRateLimiter();


// AUTHORISATION

// Enable authorisation rules used by the application
app.UseAuthorization();


// DEFAULT ROUTE

// If no page is given, open the Home page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Start the website
app.Run();