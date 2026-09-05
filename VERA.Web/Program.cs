using Microsoft.EntityFrameworkCore;
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


// BUILD APP

var app = builder.Build();


// ERROR HANDLING

// Show a normal error page instead of developer errors when deployed
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Allow the website to use CSS, JavaScript and images from wwwroot
app.UseStaticFiles();

// Enable routing between pages and controllers
app.UseRouting();

// Enable authorisation
app.UseAuthorization();


// DEFAULT ROUTE

// If no page is given, open the Home page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Start the website
app.Run();