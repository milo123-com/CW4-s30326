using System.ComponentModel.DataAnnotations;
namespace ConsoleApp2.Models
{
    public class ClientCreateDto
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName  { get; set; }
        [Required] [EmailAddress] public string Email     { get; set; }
        [Phone]                    public string Telephone { get; set; }
        [Required]
        [RegularExpression(@"\d{11}", ErrorMessage = "Pesel musi miec 11 znakow")]
        public string Pesel { get; set; }
    }
}