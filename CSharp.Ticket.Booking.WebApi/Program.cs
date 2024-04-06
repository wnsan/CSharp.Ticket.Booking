using CSharp.Ticket.Booking.DataAccess;
using CSharp.Ticket.Booking.DataAccess.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TicketDbContext>(option =>
{
    option.UseSqlServer(
        "Server=(localdb)\\TestDb;Database=TicketBooking;Trusted_Connection=True;MultipleActiveResultSets=true");


});
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = "localhost:6379";
//});

builder.Services.AddScoped<ITicketPoolRepository>(c =>
{
    var config = c.GetService<IConfiguration>();
    var db = c.GetService<TicketDbContext>();
    return new TicketPoolRepository(db, config["RedisConnection"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
