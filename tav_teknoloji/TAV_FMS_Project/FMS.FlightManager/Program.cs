using FMS.FlightManager.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FirebaseAdmin;
using FMS.Security.Authorization;
using FMS.Security.Data;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);


// PostgreSQL DbContext Servis Kaydı (Npgsql kütüphanesi ile)
builder.Services.AddDbContext<FlightDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Mevcut builder.Services tanımlamalarının arasına ekle:
builder.Services.AddDbContext<FmsAuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbConnection")));


// 1. Dinamik kural dönüştürücümüzü kaydediyoruz
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// 2. Veritabanına gidip yetki kontrolü yapacak asıl Handler sınıfımızı kaydediyoruz
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// 3. Authorization middleware'inin servislerde ekli olduğundan emin oluyoruz
builder.Services.AddAuthorization();


// FlightManager için Redis Servis Kaydı
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration");
    options.InstanceName = "FMS_";
});


// Kafka Consumer Arka Plan Servisi
builder.Services.AddHostedService<FMS.FlightManager.Services.KafkaConsumerService>();

// Kafka Producer Servisi
builder.Services.AddSingleton<FMS.FlightManager.Services.KafkaProducerService>();


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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// --- FIREBASE ADMIN SDK KURULUMU ---
// Az önce indirdiğimiz JSON dosyasını okuyarak Firebase'e tam yetkili bağlanıyoruz.
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("firebase-admin.json")
});


// Firebase JWT Authentication Ayarları
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
            
            // YENİ EKLENEN SATIR: Firebase'in gönderdiği "role" bilgisini .NET rollerine bağlar
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

// Kimlik Doğrulama (Authentication) her zaman Yetkilendirmeden (Authorization) önce gelmeli.
app.UseAuthentication();
app.UseAuthorization();

//app.UseHttpsRedirection();


app.MapControllers();

app.Run();


