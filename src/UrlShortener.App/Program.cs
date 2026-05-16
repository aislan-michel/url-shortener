using Microsoft.EntityFrameworkCore;
using UrlShortener.App.Infrastructure.BackgroundServices;
using UrlShortener.App.Infrastructure.Persistence;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddScoped<IShortUrlService, ShortUrlService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

builder.Services.AddHttpClient();
builder.Services.AddHostedService<ValidateUrlService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.ShortUrls.Any())
    {
        var baseUrl = "http://localhost:5282/";
        dbContext.ShortUrls.AddRange(
            new ShortUrl("google", "https://www.google.com", baseUrl),
            new ShortUrl("youtube", "https://www.youtube.com", baseUrl),
            new ShortUrl("wikipedia", "https://www.wikipedia.org", baseUrl, "Inactive"),
            new ShortUrl("amazon", "https://www.amazon.com", baseUrl, DateOnly.FromDateTime(DateTime.Now.AddDays(-2))),
            new ShortUrl("facebook", "https://www.facebook.com", baseUrl),
            new ShortUrl("instagram", "https://www.instagram.com", baseUrl),
            new ShortUrl("twitter", "https://www.twitter.com", baseUrl),
            new ShortUrl("linkedin", "https://www.linkedin.com", baseUrl),
            new ShortUrl("netflix", "https://www.netflix.com", baseUrl),
            new ShortUrl("spotify", "https://www.spotify.com", baseUrl),
            new ShortUrl("github", "https://www.github.com", baseUrl),
            new ShortUrl("reddit", "https://www.reddit.com", baseUrl),
            new ShortUrl("office", "https://www.office.com", baseUrl),
            new ShortUrl("whatsapp", "https://www.whatsapp.com", baseUrl),
            new ShortUrl("stackoverflow", "https://stackoverflow.com", baseUrl)
        );
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
