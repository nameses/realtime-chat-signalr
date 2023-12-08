using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Encodings.Web;
using webapi.Configuration;
using webapi.Context;
using webapi.Helper;
using webapi.Services;

var builder = WebApplication.CreateBuilder(args);
//configuration(appsettings.json)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
builder.Services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
builder.Services.Configure<PasswordEncryption>(configuration.GetSection("PasswordEncryption"));
builder.Services.Configure<MessagesPath>(configuration.GetSection("MessagesPath"));
var connString = configuration.GetSection("ConnectionStrings")["SqlConnection"];
//log services
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
////redis
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.InstanceName = "RedisInstance";
//    options.Configuration = "localhost:6379";
//});
builder.Services.AddHttpClient();
//controllers
builder.Services.AddControllers();
//services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<JwtHandler>();
builder.Services.AddTransient<JwtValidator>();
builder.Services.AddTransient<JwtGenerator>();
builder.Services.AddSingleton(UrlEncoder.Default);
builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddSingleton<Encrypter>();
builder.Services.AddSingleton<WebSocketsHandler>();
//builder.Services.AddSingleton<MessageSaverService>();
//builder.Services.AddSingleton<OnlineUserRepository>();

builder.Services.AddHttpClient();
//DbContext
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(connString));
//jwt auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<JwtBearerOptions, JwtHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });
//SignalR
//builder.Services.AddSignalR().AddMessagePackProtocol();
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

app.UseWebSockets();
//app.Map("/ws", b => b.ApplicationServices.GetService<WebSocketsHandler>());
//app.Map("/ws", builder =>
//{
//    var webSocketHandler = new WebSocketsHandler();
//    builder.Use(webSocketHandler.HandleWebSocketConnection);
//});

var webSocketHandler = new WebSocketsHandler();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" || context.Request.Path == "/wss")
    {
        if (!context.WebSockets.IsWebSocketRequest) return;//{ context.Response.StatusCode = 400; return; }
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        string username = context.Request?.Query["username"]!;
        await webSocketHandler.HandleWebSocketConnection(socket, username);
        //context.Response.StatusCode = 200;
    }
    else
    {
        await next();
    }
});



app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();
app.MapControllers();
//app.MapHub<ChatHub>("/chatsocket");

app.Run();
