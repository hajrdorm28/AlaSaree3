using AlaSaree3.Models;
using AlaSaree3.ViewModels.Admin;

namespace AlaSaree3.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<CategoryViewModel>> GetAllWithCountsAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(CategoryViewModel model);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(CategoryViewModel model);
        Task<bool> CanDeleteAsync(int id);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}
