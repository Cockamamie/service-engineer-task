using Messages.Api;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load("./.env");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.WebHost.UseKestrel(
    serverOptions =>
    {
        serverOptions.ListenAnyIP(8080);
    });

DependencyContainer.RegisterDependencies(builder.Services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
//app.UseAuthorization();

app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();