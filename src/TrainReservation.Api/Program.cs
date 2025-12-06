using TrainReservation.Api.Services;
using TrainReservation.Api.Transport;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DictionaryKeyPolicy = null;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/reservations", (ReservationRequest request, IReservationService service) =>
{
    var response = service.PlanReservation(request);
    return Results.Ok(response);
})
.WithName("PlanReservation")
.WithOpenApi();

app.Run();
