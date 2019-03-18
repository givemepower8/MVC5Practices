using System;
using System.ComponentModel.DataAnnotations;

namespace MVC5Practices.Models
{
    /// <summary>
    /// Define a class that will hold the detailed information for a book.
    /// </summary>
    public class BookDetails
    {
        [Required]
        public String Id { get; set; }
        [Required]
        public String Title { get; set; }
        public String Author { get; set; }
        public String Genre { get; set; }
        public Decimal Price { get; set; }
        public DateTime PublishDate { get; set; }
        public String Description { get; set; }
    }
}