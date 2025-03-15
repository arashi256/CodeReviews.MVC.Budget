using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TCSA_Budget.Arashi256.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Category field is required.")]
        public int CategoryId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [JsonIgnore] // Prevents circular reference.
        public Category? Category { get; set; } // Navigation property.
    }
}