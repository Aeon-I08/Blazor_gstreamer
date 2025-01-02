using Microsoft.EntityFrameworkCore;
using WebCamApp.Components;
using WebCamApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<MovieDbContext>(opt => {
	if (builder.Environment.IsDevelopment()) {
		opt = opt.EnableSensitiveDataLogging().EnableDetailedErrors();
	}
	opt.UseSqlServer(
		builder.Configuration.GetConnectionString("MovieDbContext"),
		providerOptions => {
			providerOptions.EnableRetryOnFailure();
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
