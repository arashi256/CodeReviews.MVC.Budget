using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TCSA_Budget.Arashi256.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
        public string Name { get; set; } = string.Empty;

        // Navigation property for related transactions.
        [JsonIgnore] // Prevents circular reference.
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}