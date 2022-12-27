using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TrainPassenger.Models
{
    public enum Category { AC = 1, Snigdha, Shovon, Shulov }
    public class Train
    {
        public int TrainId { get; set; }
        [Required, StringLength(40)]
        public string TrainName { get; set; } = default!;
        [Required, StringLength(40)]
        public string StationAddress { get; set; } = default!;
        [Required, StringLength(40)]
        public string Destination { get; set; } = default!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
    public class Ticket
    {
        public int TicketId { get; set; }
        [Required, Column(TypeName = "date"), Display(Name = "Journey Date"), DataType(DataType.Date)]
        public DateTime JourneyDate { get; set; } = default!;
        public Category Category { get; set; }
        [Required, Column(TypeName = "money")]
        public decimal Price { get; set; } = default!;
        public bool IsAvailable { get; set; }
        [Required, ForeignKey("Train")]
        public int TrainId { get; set; }
        public Train Train { get; set; } = default!;
        public ICollection<TicketItem> TicketItems { get; set; } = new List<TicketItem>();
    }
    public class Passenger
    {
        public int PassengerId { get; set; }
        [Required, StringLength(40)]
        public string PassengerName { get; set; } = default!;
        [Required, StringLength(40)]
        public string Phone { get; set; } = default!;
        [Required, StringLength(150)]
        public string Picture { get; set; } = default!;
        public ICollection<TicketItem> TicketItems { get; set; } = new List<TicketItem>();
    }
    public class TicketItem
    {
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; } = default!;
        [ForeignKey("Passenger")]
        public int PassengerId { get; set; }
        public virtual Passenger Passenger { get; set; } = default!;
        public int Quantity { get; set; }
    }
    public class TrainDbContext : DbContext
    {
        public TrainDbContext(DbContextOptions<TrainDbContext> options) : base(options) { }
        public DbSet<Train> Trains { get; set; } = default!;
        public DbSet<Ticket> Tickets { get; set; } = default!;
        public DbSet<Passenger> Passengers { get; set; } = default!;
        public DbSet<TicketItem> TicketItems { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketItem>()
                .HasKey(o => new { o.TicketId, o.PassengerId });
        }
    }
}
