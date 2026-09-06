using FMS.ReferenceManager.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FMS.Security.Authorization; // PermissionPolicyProvider ve PermissionHandler için
using FMS.Security.Data;
using Microsoft.AspNetCore.Authorization; // FmsAuthDbContext için

var builder = WebApplication.CreateBuilder(args);


// 1. MySQL DbContext Servis Kaydı (Pomelo MySQL kütüphanesi ile)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



// 1. Yetki veritabanı bağlantısı (PostgreSQL - fms_auth_db)
builder.Services.AddDbContext<FmsAuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbConnection")));

// 2. Dinamik kural sağlayıcı ve Handler kayıtları (İşte [HasPermission] etiketini çalıştıran beyin)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();



// 2. Şirketin istediği o kritik Redis Dağıtık Cache Servis Kaydı
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration");
    options.InstanceName = "FMS_"; // Cache anahtarlarının karışmaması için ön ek
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Firebase token'ınızı şu formatta girin: Bearer <TOKEN>"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Kafka Servisini Singleton (Tekil) olarak kaydediyoruz (Bağlantı sürekli açık kalsın diye)
builder.Services.AddSingleton<FMS.ReferenceManager.Services.KafkaProducerService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/tavfms";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/tavfms",
            ValidateAudience = true,
            ValidAudience = "tavfms",
            ValidateLifetime = true,
            
            // EKSİK OLAN KRİTİK SATIR:
            RoleClaimType = "role"
        };
    });


// SADECE BUNU BIRAKIYORSUN. İçindeki AddPolicy'leri tamamen sildik!
builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); // GÜVENLİK SIRALAMASI: Her zaman Authorization'dan önce gelmeli
app.UseAuthorization();

app.MapControllers(); // <-- Projedeki tüm controller'ları haritalayan satır budur

app.Run();


