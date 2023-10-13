using LMS.Models;
using System.Reflection;
using System.Text.Json;

namespace LMS.Services
{

    // Not needed at this time
    public enum BookStatus
    { 
        CheckedOut = 1,
        Available = 2,
    }

    // Note: In actual project this interface would be in its own separate file
    public interface IBookService
    {
        List<BooksStatus> GetAllBooksWithStatus();
        bool CheckoutBook(string bookISBN, int userId);
        bool ReturnBook(string bookISBN, int userId);
        bool RenewBook(string bookISBN, int userId);

        List<TransactionRecord> GetTransactionHistory();
        Book? GetById(int id);
        List<Book> GetAllBooks();

    }
    public class BookService : IBookService
    {
        // Data Structure for In-memory represention of book transactions
        private readonly List<TransactionRecord> records = new();

        public List<BooksStatus> GetAllBooksWithStatus()
        {
            List<BooksStatus> Books = new();

            var allBooks = GetAllBooks();

            foreach (var book in allBooks)
            {
                var history = GetTransactionHistoryByBookId(book.Id);
                var userId = history.Any() && history.Any(h => h.IsReturned == false) ? history.FirstOrDefault(h => h.IsReturned == false).UserId : 0;
                var user = GetUserById(userId);

                BooksStatus bookStatus = new()
                {
                    Id = book.Id,
                    Author = book.Author,
                    AvailabilityStatus = book.AvailabilityStatus,
                    ISBN = book.ISBN,
                    PublicationYear = book.PublicationYear,
                    Title = book.Title,
                    TotalCopies = book.TotalCopies,
                    BookStatus = history,
                    User = userId == 0 ? null : GetUserById(userId)
                };

                Books.Add(bookStatus);
            }

            return Books;
        }

        public bool CheckoutBook(string bookISBN, int userId)
        {
            Random random = new();

            var book = GetBookByISBN(bookISBN);
            if (book != null)
            {
                // Only allow checking out a book if it is available
                var isBookAvailable = records.Find(r => r.BookId == book.Id && r.IsReturned == false) == null;

                if (!isBookAvailable) return false;

                var transactionRecord = new TransactionRecord
                {
                    // Generating an Id would be handled by Server in real app
                    Id = random.Next(1, int.MaxValue),
                    BookId = book.Id,
                    UserId = userId,
                    BookCheckoutDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    // Due in 20 days
                    BookReturnDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20))
                };

                records.Add(transactionRecord);

                return true;
            }

            return false;
        }

        public bool ReturnBook(string bookISBN, int userId)
        {
            var book = GetBookByISBN(bookISBN);
            if (book != null)
            {
                TransactionRecord? result = records.FirstOrDefault(r => r.BookId == book.Id && r.UserId == userId && r.IsReturned == false);

                if (result == null)
                {
                    // todo: handle

                    return false;
                }
                else
                {
                    TransactionRecord record = new();
                    record = result;

                    record.IsReturned = true;
                    record.BookActualReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                return true;
            }

            return false;
        }


        public bool RenewBook(string bookISBN, int userId)
        {
            var book = GetBookByISBN(bookISBN);
            if (book != null)
            {
                TransactionRecord? result = records.Find(r => r.BookId == book.Id && r.UserId == userId && r.IsReturned == false);

                if (result == null)
                {
                    // todo: handle

                    return false;
                }
                // disallow renewal if already renewed for 5 consecutive times
                else if (!CanRenew(result))
                {
                    // todo: return a meaningful message
                    return false;
                }
                else
                {
                    TransactionRecord record = new();
                    record = result;

                    record.NumberOfTimesRenewed += 1;

                    // Logic can be added here to decide how far in advance the book can be renewed 
                    record.BookReturnDueDate = record.BookReturnDueDate.AddDays(20);
                }

                return true;
            }

            return false;
        }

        private bool CanRenew(TransactionRecord result)
        {
            return result.NumberOfTimesRenewed < 5;
        }


        public List<Book> GetAllBooks()
        {
            string jsonFilePath = GetFilePathByName("books.json");

            var books = ReadFromJson<Book>(jsonFilePath);

            if (books == null || !books.Any())
            { 
            // handle exception in desired way
                return new List<Book>();
            }

            return books;
        }

      
        public List<TransactionRecord> GetTransactionHistory()
        { 
            return records;
        }

        private List<TransactionRecord> GetTransactionHistoryByBookId(int bookId)
        {
            return records.FindAll(r => r.BookId == bookId);
        }

        public Book? GetById(int id)
        {
            string jsonFilePath = GetFilePathByName("books.json");

            List<Book>? Books = ReadFromJson<Book>(jsonFilePath);

            if (Books == null || !Books.Any())
            {
                // handle exception in desired way
                return null;
            }

            return Books.Find(b => b.Id == id);
        }

        public Book? GetBookByISBN(string bookISBN)
        {
            string jsonFilePath = GetFilePathByName("books.json");

            List<Book>? Books = ReadFromJson<Book>(jsonFilePath);

            if (Books == null || !Books.Any())
            {
                // handle exception in desired way
                return null;
            }

            return Books.Find(b => b.ISBN == bookISBN);
        }


        // At some point in the future,
        // I would consider putting this Function inside a separate Service - UserService
        // to respect Single Responsibility Principal
        public User? GetUserById(int id)
        {
            string jsonFilePath = GetFilePathByName("users.json");

            List<User>? Users = ReadFromJson<User>(jsonFilePath);

            if (Users == null || !Users.Any())
            {
                // handle exception in desired way
                return null;
            }

            return Users.Find(u => u.UserId == id);
        }

        
        // I would put the Functions inside the following region in a separate Service - DataService
        // to respect Single Responsibility Principal (separting the concerns/reasons this module might change in the future)
        #region DataService

        private static string GetFilePathByName(string filename)
        {
            var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            var path = Path.Combine(outPutDirectory, @$"Data\{filename}");

            string jsonFilePath = new Uri(path).LocalPath;
            return jsonFilePath;
        }

        private List<T>? ReadFromJson<T>(string filePath)
        {
            try
            {
                using FileStream fs = File.OpenRead(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                List<T> books = JsonSerializer.Deserialize<List<T>>(new StreamReader(fs).ReadToEnd(), options);
                return books ?? new List<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading JSON file: {e.Message}");
                return null;
            }
        }

        #endregion
    }
}
