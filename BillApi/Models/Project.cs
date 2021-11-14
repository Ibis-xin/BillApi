namespace BillApi.Models
{
    public class Project
    {
        public string Name { get; set; }
        public int? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public string Remark { get; set; }
    }
}