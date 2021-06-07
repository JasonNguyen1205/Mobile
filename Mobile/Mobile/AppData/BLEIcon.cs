using System;
using System.Linq;
using System.Reflection;

namespace Mobile
{
    public class BLEIcons
    {
        public Type ExampleType { get; }
        public string Title { get; }
        public string Description { get; }
        public BLEDefine? Icon { get; }

        public bool IsExample3D { get; }

        public BLEIcons(Type exampleType)
        {
            ExampleType = exampleType;

            var attribute = exampleType.GetCustomAttributes<BLEDefinition>().Single();

            IsExample3D = attribute.IsExample3D;
            Title = attribute.Title;
            Description = attribute.Description;
            Icon = attribute.Icon;
        }
    }
}