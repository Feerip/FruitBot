namespace DataTypes
{
    public class ItemDatabaseEntry
    {
        public string _itemName { get; set; }
        public string _classification { get; set; }
        public string _wikiLink { get; set; }
        public string _imageURL { get; set; }
        public bool _monitored { get; set; }
        public int? _priceOverride { get; set; }
    }
}
