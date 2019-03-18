using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVC5Practices.Models;

namespace MVC5Practices.Repositories
{ /// <summary>
    /// Define an interface which contains the methods for the book repository.
    /// </summary>
    public interface IBookRepository
    {
        BookDetails CreateBook(BookDetails book);
        IEnumerable<BookDetails> ReadAllBooks();
        BookDetails ReadBook(String id);
        BookDetails UpdateBook(String id, BookDetails book);
        Boolean DeleteBook(String id);
    }
}
