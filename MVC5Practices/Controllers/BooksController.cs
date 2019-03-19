using MVC5Practices.Models;
using MVC5Practices.Repositories;
using MVC5Practices.Utils.DataTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MVC5Practices.Controllers
{
    [RoutePrefix("api/books")]
    public class BooksController : ApiController
    {
        private readonly BookRepository _repository;

        public BooksController()
        {
            _repository = new BookRepository();
        }

        /// <summary>
        /// Method to retrieve all of the books in the catalog.
        /// Example: GET api/books
        /// </summary>
        [HttpPost]
        [Route("GetAll")]
        public IHttpActionResult GetAllBooks(DataTablesRequest request)
        {
            IEnumerable<BookDetails> books = _repository.ReadAllBooks().ToList();

            var paged = books.Skip(request.Start).Take(request.Length).ToList();

            DataTablesResult result = new DataTablesResult
            {
                Draw = request.Draw,
                RecordsFiltered = books.Count(),
                RecordsTotal = books.Count(),
                Data = paged.ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Method to retrieve a specific book from the catalog.
        /// Example: GET api/books/5
        /// </summary>
        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            BookDetails book = _repository.ReadBook(id);
            if (book != null)
            {
                return Ok(book);
            }

            return NotFound();
        }

        /// <summary>
        /// Method to add a new book to the catalog.
        /// Example: POST api/books
        /// </summary>
        [HttpPost]
        public IHttpActionResult Post(BookDetails book)
        {
            if ((ModelState.IsValid) && (book != null))
            {
                BookDetails newBook = _repository.CreateBook(book);
                if (newBook != null)
                {
                    return CreatedAtRoute("DefaultApi", new {id = newBook.Id}, newBook);
                }
            }

            return BadRequest(ModelState);
            
        }

        /// <summary>
        /// Method to update an existing book in the catalog.
        /// Example: PUT api/books/5
        /// </summary>
        [HttpPut]
        public IHttpActionResult Put(string id, BookDetails book)
        {
            if ((ModelState.IsValid) && (book != null) && (book.Id.Equals(id)))
            {
                BookDetails modifiedBook = _repository.UpdateBook(id, book);
                if (modifiedBook != null)
                {
                    return Ok(modifiedBook);
                }

                return NotFound();
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Method to remove an existing book from the catalog.
        /// Example: DELETE api/books/5
        /// </summary>
        [HttpDelete]
        public IHttpActionResult Delete(String id)
        {
            BookDetails book = _repository.ReadBook(id);
            if (book != null)
            {
                if (_repository.DeleteBook(id))
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
            return BadRequest();
        }
    }
}