using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    public class Book
    {
        [Key]
        // Using separate Id as a PK instead of ISBN for several reasons such as
        // Performance, Security, Integrity, and Uniqueness
        public int Id { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public decimal Price { get; set; }

        // This property represents total number of copies of a book that the library owns. 
        // It does NOT represent the total number of copies CURRENTLY available to be checked-out.
        public int TotalCopies { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
    }


    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    // This model represents the relationship between a User and a Book
    // This will keep record of the transactions when a user checks out, renews, or returns a book
    public class TransactionRecord
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("fk_transaction_record_user")]
        public int UserId { get; set; }
        
        [ForeignKey("fk_transaction_record_book")]
        public int BookId { get; set; }
        
        public DateOnly BookCheckoutDate { get; set; } 
        public DateOnly BookReturnDueDate { get; set; }
        public DateOnly? BookActualReturnDate { get; set; }
        public int NumberOfTimesRenewed { get; set; } = 0;
        public bool IsReturned { get; set; } = false;
    }

    public class TransactionRequest
    {
        public string BookISBN { get; set; }
        public int UserId { get; set; }
    }

    // This model can be done in several ways
    // I chose composed model over flat structure for reducing redundancy and for preserving book checkout transaction history
    public class BooksStatus: Book
    {
        public List<TransactionRecord> BookStatus { get; set; } = new() { new TransactionRecord() };
        public User User { get; set; } = new User();
    }
}
