using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using MVC5Practices.Models;

namespace MVC5Practices.Repositories
{
    /// <summary>
    /// Define a class based on the book repository interface which contains the method implementations.
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly string _xmlFilename;
        private readonly XDocument _xmlDocument;

        /// <summary>
        /// Define the class constructor.
        /// </summary>
        public BookRepository()
        {
            try
            {
                // Determine the path to the books.xml file.
                _xmlFilename = HttpContext.Current.Server.MapPath("~/app_data/books.xml");
                // Load the contents of the books.xml file into an XDocument object.
                _xmlDocument = XDocument.Load(_xmlFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Rethrow the exception.
                throw;
            }
        }

        /// <summary>
        /// Method to add a new book to the catalog.
        /// Defines the implementation of the POST method.
        /// </summary>
        public BookDetails CreateBook(BookDetails book)
        {
            try
            {
                // Retrieve the book with the highest ID from the catalog.
                var highestBook = (
                    from bookNode in _xmlDocument.Elements("catalog").Elements("book")
                    orderby bookNode.Attribute("id")?.Value descending
                    select bookNode).Take(1);
                // Extract the ID from the book data.
                string highestId = highestBook.Attributes("id").First().Value;
                // Create an ID for the new book.
                string newId = "bk" + (Convert.ToInt32(highestId.Substring(2)) + 1).ToString();
                // Verify that this book ID does not currently exist.
                if (ReadBook(newId) == null)
                {
                    // Retrieve the parent element for the book catalog.
                    XElement bookCatalogRoot = _xmlDocument.Elements("catalog").Single();
                    // Create a new book element.
                    XElement newBook = new XElement("book", new XAttribute("id", newId));
                    // Create elements for each of the book's data items.
                    XElement[] bookInfo = FormatBookData(book);
                    // Add the element to the book element.
                    newBook.ReplaceNodes(bookInfo);
                    // Append the new book to the XML document.
                    bookCatalogRoot.Add(newBook);
                    // Save the XML document.
                    _xmlDocument.Save(_xmlFilename);
                    // Return an object for the newly-added book.
                    return ReadBook(newId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Rethrow the exception.
                throw;
            }

            // Return null to signify failure.
            return null;
        }

        /// <summary>
        /// Method to retrieve all of the books in the catalog.
        /// Defines the implementation of the non-specific GET method.
        /// </summary>
        public IEnumerable<BookDetails> ReadAllBooks()
        {
            try
            {
                // Return a list that contains the catalog of book ids/titles.
                return (
                    // Query the catalog of books.
                    from book in _xmlDocument.Elements("catalog").Elements("book")
                        // Sort the catalog based on book IDs.
                    orderby book.Attribute("id")?.Value ascending
                    // Create a new instance of the detailed book information class.
                    select new BookDetails
                    {
                        // Populate the class with data from each of the book's elements.
                        Id = book.Attribute("id")?.Value,
                        Author = book.Element("author")?.Value,
                        Title = book.Element("title")?.Value,
                        Genre = book.Element("genre")?.Value,
                        Price = Convert.ToDecimal(book.Element("price")?.Value),
                        PublishDate = Convert.ToDateTime(book.Element("publish_date")?.Value),
                        Description = book.Element("description")?.Value
                    }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Rethrow the exception.
                throw;
            }
        }

        /// <summary>
        /// Method to retrieve a specific book from the catalog.
        /// Defines the implementation of the ID-specific GET method.
        /// </summary>
        public BookDetails ReadBook(string id)
        {
            try
            {
                // Retrieve a specific book from the catalog.
                return (
                    // Query the catalog of books.
                    _xmlDocument
                        .Elements("catalog")
                        .Elements("book")
                        .Where(book =>
                        {
                            var value = book?.Attribute("id")?.Value;
                            return value != null && value.Equals(id);
                        })
                        .Select(book => new BookDetails
                        {
                            // Populate the class with data from each of the book's elements.
                            Id = book.Attribute("id")?.Value,
                            Author = book.Element("author")?.Value,
                            Title = book.Element("title")?.Value,
                            Genre = book.Element("genre")?.Value,
                            Price = Convert.ToDecimal(book.Element("price")?.Value),
                            PublishDate = Convert.ToDateTime(book.Element("publish_date")?.Value),
                            Description = book.Element("description")?.Value
                        })).Single();
            }
            catch
            {
                // Return null to signify failure.
                return null;
            }
        }

        /// <summary>
        /// Populates a book BookDetails class with the data for a book.
        /// </summary>
        private static XElement[] FormatBookData(BookDetails book)
        {
            XElement[] bookInfo =
            {
                new XElement("author", book.Author),
                new XElement("title", book.Title),
                new XElement("genre", book.Genre),
                new XElement("price", book.Price.ToString(CultureInfo.InvariantCulture)),
                new XElement("publish_date", book.PublishDate.ToString(CultureInfo.InvariantCulture)),
                new XElement("description", book.Description)
            };
            return bookInfo;
        }

        /// <summary>
        /// Method to update an existing book in the catalog.
        /// Defines the implementation of the PUT method.
        /// </summary>
        public BookDetails UpdateBook(String id, BookDetails book)
        {
            try
            {
                // Retrieve a specific book from the catalog.
                XElement updateBook = _xmlDocument.XPathSelectElement($"catalog/book[@id='{id}']");
                // Verify that the book exists.
                if (updateBook != null)
                {
                    // Create elements for each of the book's data items.
                    XElement[] bookInfo = FormatBookData(book);
                    // Add the element to the book element.
                    updateBook.ReplaceNodes(bookInfo);
                    // Save the XML document.
                    _xmlDocument.Save(_xmlFilename);
                    // Return an object for the updated book.
                    return ReadBook(id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Rethrow the exception.
                throw;
            }

            // Return null to signify failure.
            return null;
        }

        /// <summary>
        /// Method to remove an existing book from the catalog.
        /// Defines the implementation of the DELETE method.
        /// </summary>
        public Boolean DeleteBook(String id)
        {
            try
            {
                if (ReadBook(id) != null)
                {
                    // Remove the specific child node from the catalog.
                    _xmlDocument
                        .Elements("catalog")
                        .Elements("book")
                        .Where(x => x.Attribute("id").Value.Equals(id))
                        .Remove();
                    // Save the XML document.
                    _xmlDocument.Save(_xmlFilename);
                    // Return a success status.
                    return true;
                }

                // Return a failure status.
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Rethrow the exception.
                throw;
            }
        }
    }
}