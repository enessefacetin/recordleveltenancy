namespace RecordLevelTenancy.Model
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int Stock { get; set; }
    }
}