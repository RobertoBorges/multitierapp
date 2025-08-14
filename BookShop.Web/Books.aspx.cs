using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Books : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadCategories();
            LoadBooks();
        }
    }

    private void LoadCategories()
    {
        try
        {
            // Sample categories - in real app this would come from CategoryService
            var categories = new List<object>
            {
                new { CategoryId = 1, Name = "Fiction" },
                new { CategoryId = 2, Name = "Non-Fiction" },
                new { CategoryId = 3, Name = "Science Fiction" },
                new { CategoryId = 4, Name = "Mystery" },
                new { CategoryId = 5, Name = "Romance" }
            };

            foreach (var category in categories)
            {
                CategoryDropDown.Items.Add(new ListItem(category.GetType().GetProperty("Name").GetValue(category, null).ToString(), 
                    category.GetType().GetProperty("CategoryId").GetValue(category, null).ToString()));
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Error loading categories: " + ex.Message, false);
        }
    }

    private void LoadBooks()
    {
        try
        {
            var books = GetSampleBooks();
            
            int selectedCategoryId = Convert.ToInt32(CategoryDropDown.SelectedValue);
            if (selectedCategoryId > 0)
            {
                books = books.Where(b => GetCategoryId(b) == selectedCategoryId).ToList();
            }

            if (books.Any())
            {
                BooksRepeater.DataSource = books;
                BooksRepeater.DataBind();
                NoResultsLabel.Visible = false;
            }
            else
            {
                BooksRepeater.DataSource = null;
                BooksRepeater.DataBind();
                NoResultsLabel.Visible = true;
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Error loading books: " + ex.Message, false);
        }
    }

    private int GetCategoryId(object book)
    {
        var categoryProp = book.GetType().GetProperty("Category");
        if (categoryProp != null)
        {
            var category = categoryProp.GetValue(book, null);
            if (category != null)
            {
                var idProp = category.GetType().GetProperty("CategoryId");
                if (idProp != null)
                {
                    return Convert.ToInt32(idProp.GetValue(category, null));
                }
            }
        }
        return 0;
    }

    protected void CategoryDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadBooks();
    }

    private void ShowMessage(string message, bool isSuccess)
    {
        MessageLabel.Text = message;
        MessageLabel.CssClass = isSuccess ? "message success" : "message error";
        MessageLabel.Visible = true;
    }

    private List<object> GetSampleBooks()
    {
        return new List<object>
        {
            new {
                BookId = 1,
                Title = "Harry Potter and the Philosopher's Stone",
                Author = new { FullName = "J.K. Rowling" },
                Category = new { CategoryId = 1, Name = "Fiction" },
                Price = 12.99m,
                StockQuantity = 50,
                ImageUrl = "~/images/books/harry-potter-1.jpg"
            },
            new {
                BookId = 2,
                Title = "The Shining",
                Author = new { FullName = "Stephen King" },
                Category = new { CategoryId = 4, Name = "Mystery" },
                Price = 14.99m,
                StockQuantity = 30,
                ImageUrl = "~/images/books/the-shining.jpg"
            },
            new {
                BookId = 3,
                Title = "Foundation",
                Author = new { FullName = "Isaac Asimov" },
                Category = new { CategoryId = 3, Name = "Science Fiction" },
                Price = 15.99m,
                StockQuantity = 20,
                ImageUrl = "~/images/books/foundation.jpg"
            },
            new {
                BookId = 4,
                Title = "Pride and Prejudice",
                Author = new { FullName = "Jane Austen" },
                Category = new { CategoryId = 5, Name = "Romance" },
                Price = 11.99m,
                StockQuantity = 60,
                ImageUrl = "~/images/books/pride-prejudice.jpg"
            },
            new {
                BookId = 5,
                Title = "1984",
                Author = new { FullName = "George Orwell" },
                Category = new { CategoryId = 1, Name = "Fiction" },
                Price = 13.99m,
                StockQuantity = 65,
                ImageUrl = "~/images/books/1984.jpg"
            },
            new {
                BookId = 6,
                Title = "Murder on the Orient Express",
                Author = new { FullName = "Agatha Christie" },
                Category = new { CategoryId = 4, Name = "Mystery" },
                Price = 13.99m,
                StockQuantity = 40,
                ImageUrl = "~/images/books/orient-express.jpg"
            }
        };
    }
}