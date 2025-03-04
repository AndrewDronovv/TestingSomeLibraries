using ChatApi.Application;
using ChatApi.Extensions;
using ChatApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangFire(builder.Configuration);
builder.Services.AddSingleton<FileRecord>();

builder.Services.AddControllers();

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddInfrastructureServices();

builder.Services.AddApplicationServices();


builder.Services.AddJwt(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();


var app = builder.Build();

app.ConfigurePipeline();

app.Run();
