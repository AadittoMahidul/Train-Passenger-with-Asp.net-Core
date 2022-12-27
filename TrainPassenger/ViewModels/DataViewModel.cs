using TrainPassenger.Models;

namespace TrainPassenger.ViewModels
{
    public class DataViewModel
    {
        public int SelectedTicketId { get; set; }
        public IEnumerable<Train> Trains { get; set; } = default!;
        public IEnumerable<Passenger> Passengers { get; set; } = default!;
        public IEnumerable<Ticket> Tickets { get; set; } = default!;
        public IEnumerable<TicketItem> TicketItems { get; set; } = default!;
    }
}
