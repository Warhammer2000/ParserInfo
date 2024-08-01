namespace Parser.Services
{
    public static class CacheKeys
    {
        public static string AllPurchases => "AllPurchases";
        public static string GetPurchases(int pageNumber, int pageSize) => $"Purchases_{pageNumber}_{pageSize}";
        public static string SearchPurchases(string searchPhrase, int pageCount, int pageNumber) => $"Purchases_Search_{searchPhrase}_{pageCount}_{pageNumber}";
    }
}
