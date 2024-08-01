using Parser.Models;

namespace Parser.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<List<Purchase>> GetPurchasesAsync(int pageNumber, int pageSize);
        Task<List<Purchase>> SearchPurchasesAsync(string searchPhrase, int pageNumber, int pageSize);
        Task<int> GetTotalPurchasesCountAsync();
        Task<bool> AnyPurchasesAsync();
        Task AddPurchasesAsync(List<Purchase> purchases);
        Task ClearAllPurchasesAsync();
    }
}
