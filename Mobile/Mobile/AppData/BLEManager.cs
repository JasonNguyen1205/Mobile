using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mobile
{
    public class BLEManager
    {
        private static BLEManager _bleManager;

        public static BLEManager Instance => _bleManager ?? (_bleManager = new BLEManager());

        public List<BLEIcons> BLE2D { get; }
        public List<BLEIcons> BLE3D { get; }
        public List<BLEIcons> Featured { get; }

        private BLEManager()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().ToList();

            BLE2D = types.Where(t => Attribute.IsDefined(t, typeof(BLEDefinition))).Select(t => new BLEIcons(t)).OrderBy(ex => ex.Title).ToList();
            BLE3D = types.Where(t => Attribute.IsDefined(t, typeof(BLEDefinition))).Select(t => new BLEIcons(t)).OrderBy(ex => ex.Title).ToList();
            Featured = types.Where(t => Attribute.IsDefined(t, typeof(BLEDefinition))).Select(t => new BLEIcons(t)).OrderBy(ex => ex.Title).ToList();
        }

        public BLEIcons GetExampleByTitle(string exampleTitle, string categoryId)
        {
            var examples = GetExamplesByCategory(categoryId);

            return examples.FirstOrDefault(x => x.Title == exampleTitle);
        }

        public List<BLEIcons> GetExamplesByCategory(string categoryId)
        {
            if (categoryId == DemoKeys.Featured)
                return Featured;

            if (categoryId == DemoKeys.Charts3D)
                return BLE3D;

            return BLE2D;
        }
    }
}