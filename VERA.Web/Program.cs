using Microsoft.EntityFrameworkCore;
using VERA.Business.Services;
using VERA.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Add MVC support.
builder.Services.AddControllersWithViews();

// Connect Entity Framework Core to Microsoft SQL Server.
builder.Services.AddDbContext<VeraDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------
// REGISTER VERA BUSINESS SERVICES
// ---------------------------------------------------------

builder.Services.AddScoped<FundingCalculatorService>();
builder.Services.AddScoped<FingerprintService>();
builder.Services.AddScoped<DuplicateDetectionService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<FulfilmentAssessmentService>();
builder.Services.AddScoped<OpportunityDecisionService>();
builder.Services.AddScoped<FulfilmentPassportService>();
builder.Services.AddScoped<OpportunityAssessmentService>();

var app = builder.Build();

// Keep the rest of your existing middleware below.