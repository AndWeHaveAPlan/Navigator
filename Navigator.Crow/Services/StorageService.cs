using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Navigator.Crow.Services
{
    public class StorageService
    {
        private readonly Dictionary<string, string> _items = new Dictionary<string, string>
        {
            {"Firebomb", "Large Titanite Shard"},
            {"Rope Firebomb", "Large Titanite Shard"},
            {"Black Firebomb", "Titanite Chunk"},
            {"Rope Black Firebomb", "Titanite Chunk"},
            {"Prism Stone", "Twinkling Titanite"},
            {"Loretta's Bone", "Ring of Sacrifice"},
            {"Avelyn", "Titanite Scale"},
            {"Coiled Sword Fragment", "Titanite Slab"},
            {"Lightning Urn", "Iron Helm"},
            {"Homeward Bone", "Iron Bracelets"},
            {"Seed of a Giant Tree", "Iron Leggings"},
            {"Siegbrau", "Armor of the Sun"},
            {"Vertebra Shackle", "Lucatiel Mask"},
            {"Divine Blessing", "\"Very Good\" Carving"},
            {"Hidden Blessing", "\"Thank You\" Carving"},
            {"Alluring Skull", "\"Hello\" Carving"},
            {"Undead Bone Shard", "Porcine Shield"},
            {"Any Sacred Chime", "Help Me Carving"},
            {"Shriving Stone", "\"I'm Sorry\" Carving"},
            {"Xanthous Crown", "Lightning Gem"},
            {"Mendicant's Staff", "Sunlight Shield"},
            {"Blacksmith Hammer", "Titanite Scale"},
            {"Large Leather Shield", "Twinkling Titanite"},
            {"Moaning Shield", "Blessed Gem"},
            {"Eleonora", "Hollow Gem"}
        };

        public StorageService(ILogger<StorageService> logger)
        {
            logger.LogInformation("Storage created");
        }

        public string Exchange(string itemName)
        {
            return _items.ContainsKey(itemName) ? _items[itemName] : null;
        }
    }
}
