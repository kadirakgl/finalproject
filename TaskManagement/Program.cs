using Serilog;

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews()
        .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();
    builder.Services.AddScoped<TaskManagement.DataAccess.DapperContext>();
    builder.Services.AddScoped<TaskManagement.DataAccess.UserRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.ProjectRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.TaskRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.IssueRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.AuditLogRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.IssueCommentRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.IssueAttachmentRepository>();
    builder.Services.AddScoped<TaskManagement.DataAccess.NotificationRepository>();
    // UserService için tüm repositoryleri parametre olarak ver
    builder.Services.AddScoped<TaskManagement.Services.UserService>(provider => new TaskManagement.Services.UserService(
        provider.GetRequiredService<TaskManagement.DataAccess.UserRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.NotificationRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.IssueRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.IssueCommentRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.IssueAttachmentRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.TaskRepository>(),
        provider.GetRequiredService<TaskManagement.DataAccess.ProjectRepository>()
    ));
    builder.Services.AddScoped<TaskManagement.Services.ProjectService>();
    builder.Services.AddScoped<TaskManagement.Services.TaskService>();
    builder.Services.AddScoped<TaskManagement.Services.NotificationService>();
    builder.Services.AddScoped<TaskManagement.Services.SeedService>();
    builder.Services.AddScoped<TaskManagement.Services.IssueService>();
    builder.Services.AddScoped<TaskManagement.Services.AuditLogService>();
    builder.Services.AddScoped<TaskManagement.Services.IssueCommentService>();
    builder.Services.AddScoped<TaskManagement.Services.IssueAttachmentService>();
    builder.Services.AddScoped<TaskManagement.Services.EmailService>();
    builder.Services.AddSignalR();
    builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = "Cookies";
        options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options => {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Account/Login";
    })
    .AddJwtBearer("JwtBearer", options => {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });
    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.Configure<RequestLocalizationOptions>(options => {
        var supportedCultures = new[] { new System.Globalization.CultureInfo("tr"), new System.Globalization.CultureInfo("en") };
        options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

    // CORS ayarları (profesyonel yapılandırma)
    builder.Services.AddCors(options =>
    {
        // Not: Production ortamında güvenlik için sadece kendi domain(ini) eklemen gerekir.
        // Şu an domainin belli değilse, geçici olarak herkese izin veriyoruz.
        // Domainin belli olunca aşağıdaki satırı:
        // policy.AllowAnyOrigin()
        // ile değiştirip, örnek:
        // policy.WithOrigins("https://senin-domainin.com").AllowAnyHeader().AllowAnyMethod();
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Demo veri seed işlemi
    using (var scope = app.Services.CreateScope())
    {
        var seedService = scope.ServiceProvider.GetService<TaskManagement.Services.SeedService>();
        seedService?.SeedDemoData();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    // Swagger UI sadece geliştirme ortamında
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    // CORS middleware'i routing'den hemen sonra eklenmeli
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>>().Value);

    app.MapHub<TaskManagement.Services.NotificationHub>("/notificationHub");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik hata oluştu!");
}
finally
{
    Log.CloseAndFlush();
}
