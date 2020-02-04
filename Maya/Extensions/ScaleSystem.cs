using System.ComponentModel;

namespace Maya2Babylon.Extensions
{
    public enum ScaleUnitType
    {
        [Description("in")]
        Inch,
        [Description("ft")]
        Foot,
        [Description("yd")]
        Yard,
        [Description("mi")]
        Mile,
        [Description("mm")]
        Millimeter,
        [Description("cm")]
        Centimeter,
        [Description("m")]
        Meter,
        [Description("km")]
        Kilometer,
    }
}