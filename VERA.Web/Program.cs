using Microsoft.EntityFrameworkCore;
using VERA.Data.Context;
using VERA.Business.Services;

var builder = WebApplication.CreateBuilder(args);


// ---------------------------------------------------------
// MVC
// ---------------------------------------------------------

builder.Services.AddControllersWithViews();


// ---------------------------------------------------------
// DATABASE
// ---------------------------------------------------------

builder.Services.AddDbContext<VeraDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));


// ---------------------------------------------------------
// VERA BUSINESS SERVICES
// ---------------------------------------------------------

builder.Services.AddScoped<FundingCalculatorService>();
builder.Services.AddScoped<FingerprintService>();
builder.Services.AddScoped<DuplicateDetectionService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<FulfilmentAssessmentService>();
builder.Services.AddScoped<OpportunityDecisionService>();
builder.Services.AddScoped<FulfilmentPassportService>();
builder.Services.AddScoped<OpportunityAssessmentService>();


// ---------------------------------------------------------
// BUILD APPLICATION
// ---------------------------------------------------------

var app = builder.Build();


// ---------------------------------------------------------
// HTTP REQUEST PIPELINE
// ---------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


// ---------------------------------------------------------
// MVC ROUTING
// ---------------------------------------------------------

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");


// ---------------------------------------------------------
// START APPLICATION
// ---------------------------------------------------------

app.Run();