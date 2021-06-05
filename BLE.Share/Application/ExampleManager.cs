using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BLE.Share.Application
{
    public class ExampleManager
    {
        private static BLE.Share.Application.ExampleManager _exampleManager;

        public static BLE.Share.Application.ExampleManager Instance => _exampleManager ?? (_exampleManager = new ExampleManager());

        public List<Example> Examples { get; }
        public List<Example> Examples3D { get; }
        public List<Example> FeaturedExamples { get; }

        private ExampleManager()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().ToList();

            Examples = types.Where(t => Attribute.IsDefined(t, typeof(BLE.Share.Application.ExampleDefinition))).Select(t => new Example(t)).OrderBy(ex => ex.Title).ToList();
            Examples3D = types.Where(t => Attribute.IsDefined(t, typeof(BLE.Share.Application.Example3DDefinition))).Select(t => new Example(t)).OrderBy(ex => ex.Title).ToList();
            FeaturedExamples = types.Where(t => Attribute.IsDefined(t, typeof(BLE.Share.Application.FeaturedExampleDefinition))).Select(t => new Example(t)).OrderBy(ex => ex.Title).ToList();
        }

        public Example GetExampleByTitle(string exampleTitle, string categoryId)
        {
            var examples = GetExamplesByCategory(categoryId);

            return examples.FirstOrDefault(x => x.Title == exampleTitle);
        }

        public List<Example> GetExamplesByCategory(string categoryId)
        {
            if (categoryId == BLE.Share.Application.DemoKeys.Featured)
                return FeaturedExamples;

            if (categoryId == BLE.Share.Application.DemoKeys.Charts3D)
                return Examples3D;

            return Examples;
        }
    }
}