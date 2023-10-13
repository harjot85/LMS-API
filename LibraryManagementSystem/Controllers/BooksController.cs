using LMS.Models;
using LMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [ApiController]
    [Route("api/v1/books/")]
    public class BooksController : ControllerBase
    {
      
        private readonly ILogger<BooksController> _logger;
        private readonly IBookService _bookService;

        public BooksController(ILogger<BooksController> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<IEnumerable<Book>> GetAllBooks()
        {
            var serviceResponse = _bookService.GetAllBooks();
            return Ok(serviceResponse);
        }

        [HttpGet]
        [Route("status")]
        public ActionResult<IEnumerable<Book>> GetBooksStatus()
        {
            var serviceResponse = _bookService.GetAllBooksWithStatus();
            return Ok(serviceResponse);
        }


        // Only for testing, will not be needed for the application
        [HttpGet]
        [Route("transations")]
        public ActionResult<IEnumerable<Book>> GetTransactionHistory()
        {
            var serviceResponse = _bookService.GetTransactionHistory();
            return Ok(serviceResponse);
        }


        // NOTE: Following 3 endpoints could also be just one endpoint that take another parameter [action - (checkout/renew/return)],
        // makes decision based on the param, and calls respective service method 

        [HttpPost]
        [Route("checkout")]
        public ActionResult<IEnumerable<Book>> CheckoutBook([FromBody]TransactionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.BookISBN) || request.UserId < 1)
                return BadRequest("invalid request. Required parameters are missing");

            var serviceResponse = _bookService.CheckoutBook(request.BookISBN, request.UserId);
            return Ok(serviceResponse);
        }

        [HttpPatch]
        [Route("return")]
        public ActionResult<IEnumerable<Book>> ReturnBook([FromBody] TransactionRequest request)
        {
            var serviceResponse = _bookService.ReturnBook(request.BookISBN, request.UserId);
            return Ok(serviceResponse);
        }

        [HttpPatch]
        [Route("renew")]
        public ActionResult<IEnumerable<Book>> RenewBook([FromBody] TransactionRequest request)
        {
            var serviceResponse = _bookService.RenewBook(request.BookISBN, request.UserId);
            return Ok(serviceResponse);
        }
    }
}