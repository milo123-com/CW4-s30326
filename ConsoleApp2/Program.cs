var builder = WebApplication.CreateBuilder(args);

// Dodajemy kontrolery i Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();
app.MapControllers();
app.Run();