using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadFeaturedBooks();
        }
    }

    private void LoadFeaturedBooks()
    {
        try
        {
            // In a real application, we would use the BookService here
            // For now, create some sample data for demonstration
            var featuredBooks = CreateSampleBooks();
            FeaturedBooksRepeater.DataSource = featuredBooks.Take(4); // Show first 4 books
            FeaturedBooksRepeater.DataBind();
        }
        catch (Exception ex)
        {
            // Log error and show user-friendly message
            Response.Write("<div class='message error'>Error loading featured books: " + ex.Message + "</div>");
        }
    }

    private List<object> CreateSampleBooks()
    {
        // Sample data for demonstration - in real app this would come from BookService
        return new List<object>
        {
            new {
                BookId = 1,
                Title = "Harry Potter and the Philosopher's Stone",
                Author = new { FullName = "J.K. Rowling" },
                Category = new { Name = "Fiction" },
                Price = 12.99m,
                ImageUrl = "~/images/books/harry-potter-1.jpg"
            },
            new {
                BookId = 2,
                Title = "The Shining",
                Author = new { FullName = "Stephen King" },
                Category = new { Name = "Horror" },
                Price = 14.99m,
                ImageUrl = "~/images/books/the-shining.jpg"
            },
            new {
                BookId = 3,
                Title = "Pride and Prejudice",
                Author = new { FullName = "Jane Austen" },
                Category = new { Name = "Romance" },
                Price = 11.99m,
                ImageUrl = "~/images/books/pride-prejudice.jpg"
            },
            new {
                BookId = 4,
                Title = "1984",
                Author = new { FullName = "George Orwell" },
                Category = new { Name = "Fiction" },
                Price = 13.99m,
                ImageUrl = "~/images/books/1984.jpg"
            }
        };
    }
}