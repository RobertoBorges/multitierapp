using BookShop.Application.Interfaces;
using BookShop.Core.Interfaces;
using BookShop.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookService> _logger;

        public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            try
            {
                _logger.LogInformation("Getting all books");
                return await _bookRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all books");
                throw;
            }
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid book ID: {BookId}", id);
                    throw new ArgumentException("Book ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Getting book with ID: {BookId}", id);
                return await _bookRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID: {BookId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    _logger.LogWarning("Invalid category ID: {CategoryId}", categoryId);
                    throw new ArgumentException("Category ID must be greater than zero", nameof(categoryId));
                }

                _logger.LogInformation("Getting books by category ID: {CategoryId}", categoryId);
                return await _bookRepository.GetBooksByCategoryAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by category ID: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            try
            {
                if (authorId <= 0)
                {
                    _logger.LogWarning("Invalid author ID: {AuthorId}", authorId);
                    throw new ArgumentException("Author ID must be greater than zero", nameof(authorId));
                }

                _logger.LogInformation("Getting books by author ID: {AuthorId}", authorId);
                return await _bookRepository.GetBooksByAuthorAsync(authorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by author ID: {AuthorId}", authorId);
                throw;
            }
        }

        public async Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count)
        {
            try
            {
                if (count <= 0)
                {
                    _logger.LogWarning("Invalid count for featured books: {Count}", count);
                    throw new ArgumentException("Count must be greater than zero", nameof(count));
                }

                _logger.LogInformation("Getting {Count} featured books", count);
                return await _bookRepository.GetFeaturedBooksAsync(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured books");
                throw;
            }
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching books with term: {SearchTerm}", searchTerm ?? "Empty");
                return await _bookRepository.SearchBooksAsync(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books with term: {SearchTerm}", searchTerm ?? "Empty");
                throw;
            }
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            try
            {
                if (book == null)
                {
                    _logger.LogWarning("Attempted to add null book");
                    throw new ArgumentNullException(nameof(book));
                }

                ValidateBook(book);

                _logger.LogInformation("Adding new book: {Title}", book.Title);
                book.CreatedDate = DateTime.UtcNow;
                book.ModifiedDate = DateTime.UtcNow;
                return await _bookRepository.AddAsync(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book: {Title}", book?.Title ?? "Unknown");
                throw;
            }
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            try
            {
                if (book == null)
                {
                    _logger.LogWarning("Attempted to update null book");
                    throw new ArgumentNullException(nameof(book));
                }

                if (book.BookId <= 0)
                {
                    _logger.LogWarning("Invalid book ID for update: {BookId}", book.BookId);
                    throw new ArgumentException("Book ID must be greater than zero", nameof(book.BookId));
                }

                ValidateBook(book);

                _logger.LogInformation("Updating book ID: {BookId}, Title: {Title}", book.BookId, book.Title);
                book.ModifiedDate = DateTime.UtcNow;
                return await _bookRepository.UpdateAsync(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book ID: {BookId}", book?.BookId ?? 0);
                throw;
            }
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid book ID for deletion: {BookId}", id);
                    throw new ArgumentException("Book ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Deleting book ID: {BookId}", id);
                return await _bookRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book ID: {BookId}", id);
                throw;
            }
        }

        private void ValidateBook(Book book)
        {
            if (string.IsNullOrEmpty(book.Title))
            {
                throw new ArgumentException("Book title is required", nameof(book.Title));
            }

            if (string.IsNullOrEmpty(book.ISBN))
            {
                throw new ArgumentException("Book ISBN is required", nameof(book.ISBN));
            }

            if (book.Price <= 0)
            {
                throw new ArgumentException("Book price must be greater than zero", nameof(book.Price));
            }
        }

        public async Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<int> ids)
        {
            try
            {
                if (ids == null)
                {
                    _logger.LogWarning("Attempted to get books with null ids collection");
                    throw new ArgumentNullException(nameof(ids));
                }

                _logger.LogInformation("Getting books by multiple IDs");
                return await _bookRepository.GetBooksByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by multiple IDs");
                throw;
            }
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(int? categoryId = null, int? authorId = null, string searchTerm = null, int page = 1, int pageSize = 12)
        {
            try
            {
                if (page < 1)
                {
                    _logger.LogWarning("Invalid page number: {Page}", page);
                    throw new ArgumentException("Page must be greater than zero", nameof(page));
                }

                if (pageSize < 1)
                {
                    _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
                    throw new ArgumentException("Page size must be greater than zero", nameof(pageSize));
                }

                _logger.LogInformation("Getting books with filters - Category: {CategoryId}, Author: {AuthorId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}",
                    categoryId, authorId, searchTerm, page, pageSize);
                
                return await _bookRepository.GetBooksAsync(categoryId, authorId, searchTerm, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books with filters");
                throw;
            }
        }

        public async Task<int> GetBooksCountAsync(int? categoryId = null, int? authorId = null, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting books count with filters - Category: {CategoryId}, Author: {AuthorId}, SearchTerm: {SearchTerm}",
                    categoryId, authorId, searchTerm);
                
                return await _bookRepository.GetBooksCountAsync(categoryId, authorId, searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books count with filters");
                throw;
            }
        }
    }
}
