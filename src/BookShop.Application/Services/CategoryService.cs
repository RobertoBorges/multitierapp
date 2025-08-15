using BookShop.Application.Interfaces;
using BookShop.Core.Interfaces;
using BookShop.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Getting all categories");
                return await _categoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Getting active categories");
                return await _categoryRepository.GetActiveCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active categories");
                throw;
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid category ID: {CategoryId}", id);
                    throw new ArgumentException("Category ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Getting category with ID: {CategoryId}", id);
                return await _categoryRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    _logger.LogWarning("Attempted to add null category");
                    throw new ArgumentNullException(nameof(category));
                }

                ValidateCategory(category);

                _logger.LogInformation("Adding new category: {Name}", category.Name);
                category.CreatedDate = DateTime.UtcNow;
                category.ModifiedDate = DateTime.UtcNow;
                return await _categoryRepository.AddAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category: {Name}", category?.Name ?? "Unknown");
                throw;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    _logger.LogWarning("Attempted to update null category");
                    throw new ArgumentNullException(nameof(category));
                }

                if (category.CategoryId <= 0)
                {
                    _logger.LogWarning("Invalid category ID for update: {CategoryId}", category.CategoryId);
                    throw new ArgumentException("Category ID must be greater than zero", nameof(category.CategoryId));
                }

                ValidateCategory(category);

                _logger.LogInformation("Updating category ID: {CategoryId}, Name: {Name}", 
                    category.CategoryId, category.Name);
                category.ModifiedDate = DateTime.UtcNow;
                return await _categoryRepository.UpdateAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", category?.CategoryId ?? 0);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid category ID for deletion: {CategoryId}", id);
                    throw new ArgumentException("Category ID must be greater than zero", nameof(id));
                }

                _logger.LogInformation("Deleting category ID: {CategoryId}", id);
                return await _categoryRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID: {CategoryId}", id);
                throw;
            }
        }

        private void ValidateCategory(Category category)
        {
            if (string.IsNullOrEmpty(category.Name))
            {
                throw new ArgumentException("Category name is required", nameof(category.Name));
            }
        }
    }
}
