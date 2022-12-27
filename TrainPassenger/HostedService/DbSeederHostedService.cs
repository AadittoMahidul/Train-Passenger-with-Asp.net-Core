using TrainPassenger.Models;

namespace TrainPassenger.HostedService
{
    public class DbSeederHostedService : IHostedService
    {      
        IServiceProvider serviceProvider;
        public DbSeederHostedService(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;          
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {               
                  var db = scope.ServiceProvider.GetRequiredService<TrainDbContext>();
                  await SeedDbAsync(db);               
            }
        }
       public async Task SeedDbAsync(TrainDbContext db)
        {
            await db.Database.EnsureCreatedAsync();
            if (!db.Trains.Any())
            {
                var t1 = new Train { TrainName = "Turna Express", StationAddress = "Kamlapur", Destination = "Chittagong" };
                await db.Trains.AddAsync(t1);
                var p1 = new Passenger { PassengerName = "Passenger 1", Phone = "01927******", Picture = "1.jpg" };
                await db.Passengers.AddAsync(p1);
                var tk1 = new Ticket { JourneyDate = DateTime.Today.AddDays(2), Category = Category.Snigdha, IsAvailable = true, Price = 1200.00M, Train = t1 };
                tk1.TicketItems.Add(new TicketItem { Ticket = tk1, Passenger = p1, Quantity = 2 });
                await db.Tickets.AddAsync(tk1);
                await db.SaveChangesAsync();
            }

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
    }
}
