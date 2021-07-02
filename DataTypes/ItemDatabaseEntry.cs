using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes
{
    public class ItemDatabaseEntry
    {
        //public string _itemName { get { return _itemName.ToLower(); } set { } }
        public string _itemName { get; set; }
        //public string _classification { get { return _classification.ToLower(); } set { } }
        public string _classification { get; set; }
        public string _wikiLink { get; set; }
        public string _imageURL { get; set; }
    }
}
