using CourseProgect_Planeta35.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "Planeta35 API");

app.MapGet("/asset/{id}", async (int id, AppDbContext db) =>
{
    var asset = await db.Assets
        .Include(a => a.Category)
        .Include(a => a.Department)
        .Include(a => a.Responsible)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (asset == null) return Results.NotFound();

    string labelDesc = "Описание";
    string labelCat = "Категория";
    string labelDep = "Подразделение";
    string labelResp = "Ответственный";
    string labelStatus = "Статус";

    return Results.Content($$"""
    <html>
    <head>
        <meta charset="utf-8">
        <title>{{asset.Name}}</title>
        <style>
            body { font-family: Arial, sans-serif; background: #f5f5f5; padding: 40px; }
            .card { background: white; border-radius: 20px; padding: 30px; max-width: 700px; margin: auto; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
            h1 { margin-bottom: 20px; }
            .row { margin-bottom: 10px; }
        </style>
    </head>
    <body>
        <div class="card">
            <h1>{{asset.Name}}</h1>
            <div class='row'><b>{{labelDesc}}:</b> {{asset.Description}}</div>
            <div class='row'><b>{{labelCat}}:</b> {{asset.Category?.Name}}</div>
            <div class='row'><b>{{labelDep}}:</b> {{asset.Department?.Name}}</div>
            <div class='row'><b>{{labelResp}}:</b> {{asset.Responsible?.FullName}}</div>
            <div class='row'><b>{{labelStatus}}:</b> {{asset.Status}}</div>
        </div>
    </body>
    </html>
    """, "text/html; charset=utf-8");
});

app.Run();