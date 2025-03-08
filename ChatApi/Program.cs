using ChatApi.Application;
using ChatApi.Extensions;
using ChatApi.Infrastructure.Extensions;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangFire(builder.Configuration);
builder.Services.AddSingleton<TaskFileRecord>();
builder.Services.AddSingleton<TokenFileRecord>();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();


builder.Services.AddControllers();

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddInfrastructureServices();

builder.Services.ConfigureCors();

builder.Services.AddApplicationServices();
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();


var app = builder.Build();

app.ConfigurePipeline();

app.Run();
