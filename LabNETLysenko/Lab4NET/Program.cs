using YoloParser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


Parser pars = new(new Services());

builder.Services.AddSingleton(pars);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole(); // Логирование в консоль
    loggingBuilder.AddDebug();   // Логирование в debug window
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddCors(o => o.AddPolicy("Cors", ));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseCors(builder =>
{
    builder
        .WithOrigins("*")
        .WithHeaders("*")
        .WithMethods("*");
});

app.MapControllers();

app.Run();


public class Services : YoloParser.IServices
{
    public bool Exists(string path) => File.Exists(path);
    public void WriteBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
    public void Print(string msg) => Console.WriteLine(msg);

}