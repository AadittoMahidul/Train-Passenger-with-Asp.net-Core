using System.ComponentModel.DataAnnotations;

namespace TrainPassenger.ViewModels
{
    public class PassengerEditModel
    {
        public int PassengerId { get; set; }
        [Required, StringLength(40)]
        public string PassengerName { get; set; } = default!;
        [Required, StringLength(40)]
        public string Phone { get; set; } = default!;
        
        public IFormFile Picture { get; set; } = default!;
    }
}
