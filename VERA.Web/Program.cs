using Microsoft.EntityFrameworkCore;
using VERA.Business.Services;
using VERA.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// ADD MVC SUPPORT
// ---------------------------------------------------------
//
// Enables Controllers and Razor Views for the VERA web app.
builder.Services.AddControllersWithViews();


// ---------------------------------------------------------
// DATABASE CONNECTION
// ---------------------------------------------------------
//
// Connect VERA to the Microsoft SQL Server database using
// the connection string stored in appsettings.json.
builder.Services.AddDbContext<VeraDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ---------------------------------------------------------
// REGISTER VERA BUSINESS SERVICES
// ---------------------------------------------------------
//
// These services contain VERA's core opportunity assessment
// and fulfilment logic.
//
// AddScoped means ASP.NET Core creates one instance of each
// service for each incoming web request.

builder.Services.AddScoped<FundingCalculatorService>();

builder.Services.AddScoped<FingerprintService>();

builder.Services.AddScoped<DuplicateDetectionService>();

builder.Services.AddScoped<VerificationService>();

builder.Services.AddScoped<FulfilmentAssessmentService>();

builder.Services.AddScoped<OpportunityDecisionService>();

builder.Services.AddScoped<FulfilmentPassportService>();

// This is the main orchestration service.
//
// Controllers can call this one service to run:
// funding calculations
// -> fingerprint generation
// -> duplicate detection
// -> verification
// -> fulfilment assessment
// -> final readiness decision.
builder.Services.AddScoped<OpportunityAssessmentService>();


// ---------------------------------------------------------
// BUILD THE APPLICATION
// ---------------------------------------------------------

var app = builder.Build();


// ---------------------------------------------------------
// ERROR HANDLING
// ---------------------------------------------------------
//
// In production, users should not see detailed developer
// exception information.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Adds HTTP Strict Transport Security in production.
    app.UseHsts();
}


// ---------------------------------------------------------
// HTTPS
// ---------------------------------------------------------
//
// Redirect HTTP traffic to HTTPS.
app.UseHttpsRedirection();


// ---------------------------------------------------------
// STATIC FILES
// ---------------------------------------------------------
//
// Allows VERA to serve files from wwwroot such as:
// CSS
// JavaScript
// images
// logos
app.UseStaticFiles();


// ---------------------------------------------------------
// ROUTING
// ---------------------------------------------------------

app.UseRouting();


// ---------------------------------------------------------
// AUTHORISATION
// ---------------------------------------------------------
//
// This prepares the application for controller/page
// authorisation rules.
//
// Authentication can be added separately if the team
// implements login during the MVP.
app.UseAuthorization();


// ---------------------------------------------------------
// DEFAULT MVC ROUTE
// ---------------------------------------------------------
//
// Example:
//
// /Opportunity/Details/5
//
// Controller = Opportunity
// Action     = Details
// id         = 5
//
// If no controller/action is supplied, the application
// opens Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ---------------------------------------------------------
// START VERA
// ---------------------------------------------------------

app.Run();