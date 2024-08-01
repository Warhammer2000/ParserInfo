namespace Parser.Models
{
    public class PurchaseViewModel
    {
        public IEnumerable<Purchase> Purchases { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchPhrase { get; set; }
        public int PageCount { get; set; }
    }
}
