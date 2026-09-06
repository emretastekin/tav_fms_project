using FMS.ArchiveManager.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FMS.Security.Authorization; // PermissionPolicyProvider ve PermissionHandler'ın olduğu yer
using FMS.Security.Data;
using Microsoft.AspNetCore.Authorization; // FmsAuthDbContext'in olduğu yer


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ArchiveDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Yetki veritabanı bağlantısı (FmsAuthDbContext)
builder.Services.AddDbContext<FmsAuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDbConnection")));

// Dinamik kural dönüştürücümüz ve Yetki Kontrolcümüz (İşte 500 hatasını çözen kısım!)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Controller'ları servislere ekle
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


// Kafka Consumer Servisi (Build'den önce olmalı!)
builder.Services.AddHostedService<FMS.ArchiveManager.Services.KafkaArchiveConsumer>();


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
            // YENİ EKLENEN SATIR: Firebase'in gönderdiği "role" bilgisini .NET rollerine bağlar
            RoleClaimType = "role"
        };
    });

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

app.MapControllers(); // Controller'ları kullanabilmek için bu satırı da ekle



app.Run();

