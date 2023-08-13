using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Encodings.Web;
using webapi.Configuration;
using webapi.Context;
using webapi.Helper;
using webapi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
//configuration(appsettings.json)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
builder.Services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
builder.Services.Configure<PasswordEncryption>(configuration.GetSection("PasswordEncryption"));
builder.Services.Configure<MessagesPath>(configuration.GetSection("MessagesPath"));
var connString = configuration.GetSection("ConnectionStrings")["SqlConnection"];

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
//controllers
builder.Services.AddControllers();
//services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOnlineUserRepository, OnlineUserRepository>();
builder.Services.AddTransient<JwtHandler>();
builder.Services.AddTransient<JwtGenerator>();
builder.Services.AddSingleton(UrlEncoder.Default);
builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddSingleton<Encrypter>();
builder.Services.AddSingleton<MessageSaverService>();

builder.Services.AddHttpClient();
//DbContext
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(connString));
//jwt auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<JwtBearerOptions, JwtHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });
//SignalR
builder.Services.AddSignalR().AddMessagePackProtocol();
//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv6", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
            });
});
//CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost")
            .AllowCredentials()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthorization();

//app.UseWebSockets();

app.UseCors(x => x.WithOrigins("https://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ChatHub>("/chatsocket");

app.Run();