using BookShop.Application.Interfaces;
using BookShop.Core.Interfaces;
using BookShop.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<AuthorService> _logger;

        public AuthorService(IAuthorRepository authorRepository, ILogger<AuthorService> logger)
        {
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all authors");
                return await _authorRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all authors");
                throw;
            }
        }

        public async Task<IEnumerable<Author>> GetActiveAuthorsAsync()
        {
            try
            {
                _logger.LogInformation("Getting active authors");
                return await _authorRepository.GetActiveAuthorsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active authors");
                throw;
            }
        }

        public async Task<Author> GetAuthorByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid author ID: {AuthorId}", id);
                    throw new ArgumentException("Author ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Getting author with ID: {AuthorId}", id);
                return await _authorRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving author with ID: {AuthorId}", id);
                throw;
            }
        }

        public async Task<Author> AddAuthorAsync(Author author)
        {
            try
            {
                if (author == null)
                {
                    _logger.LogWarning("Attempted to add null author");
                    throw new ArgumentNullException(nameof(author));
                }

                ValidateAuthor(author);

                _logger.LogInformation("Adding new author: {FirstName} {LastName}", author.FirstName, author.LastName);
                author.CreatedDate = DateTime.UtcNow;
                author.ModifiedDate = DateTime.UtcNow;
                return await _authorRepository.AddAsync(author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding author: {FirstName} {LastName}", 
                    author?.FirstName ?? "Unknown", author?.LastName ?? "Unknown");
                throw;
            }
        }

        public async Task<bool> UpdateAuthorAsync(Author author)
        {
            try
            {
                if (author == null)
                {
                    _logger.LogWarning("Attempted to update null author");
                    throw new ArgumentNullException(nameof(author));
                }

                if (author.AuthorId <= 0)
                {
                    _logger.LogWarning("Invalid author ID for update: {AuthorId}", author.AuthorId);
                    throw new ArgumentException("Author ID must be greater than zero", nameof(author.AuthorId));
                }

                ValidateAuthor(author);

                _logger.LogInformation("Updating author ID: {AuthorId}, Name: {FirstName} {LastName}", 
                    author.AuthorId, author.FirstName, author.LastName);
                author.ModifiedDate = DateTime.UtcNow;
                return await _authorRepository.UpdateAsync(author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author ID: {AuthorId}", author?.AuthorId ?? 0);
                throw;
            }
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid author ID for deletion: {AuthorId}", id);
                    throw new ArgumentException("Author ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Deleting author ID: {AuthorId}", id);
                return await _authorRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author ID: {AuthorId}", id);
                throw;
            }
        }

        private void ValidateAuthor(Author author)
        {
            if (string.IsNullOrEmpty(author.FirstName))
            {
                throw new ArgumentException("Author first name is required", nameof(author.FirstName));
            }

            if (string.IsNullOrEmpty(author.LastName))
            {
                throw new ArgumentException("Author last name is required", nameof(author.LastName));
            }
        }
    }
}
