using System;

namespace Mobile
{
    public enum BLEDefine
    {
        Annotations,
        Axis,
        BandChart,
        BubbleChart,
        CandlestickChart,
        ColumnChart,
        CubeChart,
        DigitalLine,
        ErrorBars,
        Fan,
        FeatureChart,
        HeatmapChart,
        Impulse,
        LineChart,
        MountainChart,
        Ohlc,
        PieChart,
        RealTime,
        ScatterChart,
        StackedBar,
        StackedColumn,
        StackedColumns100,
        StackedMountainChart,
        Themes,
        ZoomPan,

        Axis3D,
        Scatter3D,
        Surface3D,
    }

    public class BLEDefinition : Attribute
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public BLEDefine? Icon { get; set; }
        public bool IsExample3D { get; }

        public BLEDefinition(bool isExample3D, string title, string description, BLEDefine icon)
        {
            IsExample3D = isExample3D;
            Title = title;
            Description = description;
            Icon = icon;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Definition2D : BLEDefinition
    {
        public Definition2D(string title, string description, BLEDefine icon = default) : base(false, title, description, icon)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Definition3D : BLEDefinition
    {
        public Definition3D(string title, string description, BLEDefine icon = default) : base(true, title, description, icon)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Featured : BLEDefinition
    {
        public Featured(string title, string description, BLEDefine icon = default) : base(true, title, description, icon)
        {
        }
    }
}