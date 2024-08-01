namespace Parser.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public string PurchaseNumber { get; set; }
        public string Title { get; set; }
        public string Organizer { get; set; }
        public string Price { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
    }
}
