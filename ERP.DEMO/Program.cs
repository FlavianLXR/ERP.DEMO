using ERP.DEMO.Components;
using ERP.DEMO.Components.Tools;
using ERP.DEMO.Components.Tools.DataGrid;
using ERP.DEMO.Components.MVVM;
using ERP.DEMO.Models;
using ERP.DEMO.Models.DataAccessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ERP.DEMO.Components.Pages.Account;
using ApexCharts;
using Microsoft.JSInterop;
using Tools = ERP.DEMO.Components.Tools.Tools;
using Microsoft.Win32;
using ERP.DEMO.Models.TestDb;
using MudBlazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor();

//LOGIN
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
		provider.GetRequiredService<LoginService>());
builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddAuthentication("authUser").AddCookie(options =>
{
	options.Cookie.Name = "auth_token";
	options.LoginPath = "/login";
	options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
	options.AccessDeniedPath = "/login";
});

builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.PreventDuplicates = false;
	config.SnackbarConfiguration.NewestOnTop = false;
	config.SnackbarConfiguration.ShowCloseIcon = true;
	config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
	config.SnackbarConfiguration.ClearAfterNavigation = true;
	config.SnackbarConfiguration.VisibleStateDuration = 5000;
	config.SnackbarConfiguration.HideTransitionDuration = 500;
	config.SnackbarConfiguration.ShowTransitionDuration = 500;
	config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddDbContextFactory<TestDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("TestDbContext") ?? throw new InvalidOperationException("Connection string 'TestDbContext' not found.")));

builder.Services.AddScoped<IDbContextResolver, DbContextResolver>();
//builder.Services.AddScoped(typeof(GenericService<>));
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
//builder.Services.AddScoped<MovementService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoggerService>();
//builder.Services.AddScoped<FileService>();

builder.Services.AddSignalR(hubOptions =>
{
	hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
	hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
});

builder.Services.AddMudServices();
builder.Services.AddApexCharts(e =>
{
	e.GlobalOptions = new ApexChartBaseOptions
	{
		Debug = true,
		Theme = new ApexCharts.Theme { Palette = PaletteType.Palette6 }
	};
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UsePathBase("/ERP.DEMO");
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
{
	using (var scope = app.Services.CreateScope())
	{
		var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        //context.Database.EnsureDeleted();  // Supprime toute la BDD
        //context.Database.EnsureCreated(); // La recrée proprement
        context.Database.Migrate(); // ← applique les migrations au lieu de EnsureCreated

        DbSeederService.Seed(context);
	}

}

app.Run();
