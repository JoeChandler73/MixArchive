using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MixArchiveDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<MixScanner>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

app.MapGet(
    "/scan",
    async (MixScanner scanner) =>
    {
        var result = await scanner.ScanAsync();

        return Results.Ok(result);
    }
);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
