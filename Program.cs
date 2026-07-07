using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
builder.Services.AddDbContext<NomcatDbContext>(options =>
    options.UseSqlite("Data Source=nomcat.db"));

// --- Services ---
builder.Services.AddSingleton<TicketGenerator>();
builder.Services.AddSingleton<DescriptionEngine>();
builder.Services.AddSingleton<LifecycleRouter>();
builder.Services.AddScoped<DuplicateDetector>();
builder.Services.AddScoped<ExportBridge>();
builder.Services.AddScoped<NomBotService>();
builder.Services.AddHttpClient<GeminiService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
});

// --- Controllers ---
builder.Services.AddControllers();

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- Create DB and seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NomcatDbContext>();
    db.Database.EnsureCreated();
    DatabaseSeeder.SeedGoldenCatalog(db, scope.ServiceProvider.GetRequiredService<DescriptionEngine>());
}

// --- Middleware ---
app.UseCors();

// Landing page for root URL
app.MapGet("/", () => Microsoft.AspNetCore.Http.Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>NOMCAT MDG Engine</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background: #0d1117;
            color: #c9d1d9;
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
            margin: 0;
        }
        .container {
            text-align: center;
            background: #161b22;
            border: 1px solid #30363d;
            border-radius: 8px;
            padding: 3rem;
            box-shadow: 0 8px 24px rgba(0,0,0,0.5);
            max-width: 450px;
        }
        h1 {
            color: #8b5cf6;
            margin-top: 0;
            font-size: 1.8rem;
        }
        p {
            font-size: 0.95rem;
            line-height: 1.5;
            color: #8b949e;
        }
        code {
            background: #21262d;
            padding: 0.2rem 0.4rem;
            border-radius: 4px;
            font-family: ui-monospace, SFMono-Regular, SF Mono, Menlo, Consolas, Liberation Mono, monospace;
            font-size: 85%;
            color: #58a6ff;
        }
        .btn {
            display: inline-block;
            background: linear-gradient(135deg, #6366f1, #8b5cf6);
            color: #ffffff;
            padding: 0.6rem 1.2rem;
            border-radius: 6px;
            text-decoration: none;
            font-weight: 600;
            font-size: 0.9rem;
            margin-top: 1.5rem;
            box-shadow: 0 4px 12px rgba(99, 102, 241, 0.25);
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 6px 16px rgba(99, 102, 241, 0.35);
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🤖 NOMCAT MDG Engine</h1>
        <p>The Master Data Governance backend API is online and fully functional.</p>
        <p>Listening on <code>http://localhost:5181</code></p>
        <a href='http://localhost:5173' class='btn'>Launch Frontend Workspace</a>
    </div>
</body>
</html>", "text/html"));

app.MapControllers();

// --- Configure port ---
app.Urls.Add("http://localhost:5181");

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║        NOMCAT — Master Data Governance Engine       ║");
Console.WriteLine("║        Backend API running on :5181                 ║");
Console.WriteLine("║        NomBot Chat Service Activated                ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");

app.Run();
