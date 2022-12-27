using System.ComponentModel.DataAnnotations;

namespace TrainPassenger.ViewModels
{
    public class PassengerInputModel
    {
        public int PassengerId { get; set; }
        [Required, StringLength(40)]
        public string PassengerName { get; set; } = default!;
        [Required, StringLength(40)]
        public string Phone { get; set; } = default!;
        [Required]
        public IFormFile Picture { get; set; } = default!;
    }
}
