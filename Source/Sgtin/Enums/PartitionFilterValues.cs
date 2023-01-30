namespace Sgtin.Enums;

public static class PartitionFilterValues
{
    public static Dictionary<int, string> SgtinFilterValues = new Dictionary<int, string>()
    {
        {0 , "Other"},
        {1, "PointOfSale"},
        {2, "FullCaseForTransport"},
        {3, "Reserved"},
        {4, "InnerPackTradeItemGroupingForHandling"},
        {5, "Reserved"},
        {6, "UnitLoad"},
        {7, "UnitInsideTradeItem"},
    };
    
    public static Dictionary<int, int[]> Sgtin96PartitionMap = new Dictionary<int, int[]>()
    {
        { 0, new[] { 40, 4 } },
        { 1, new[] { 37, 7 } },
        { 2, new[] { 34, 10 } },
        { 3, new[] { 30, 14 } },
        { 4, new[] { 27, 17 } },
        { 5, new[] { 24, 20 } },
        { 6, new[] { 20, 24 } }
    };

    public static int GetPartitionFromCompanyPrefix(int prefixLength)
    {
        if (prefixLength < 6)
        {
            return 6;
        }

        if (prefixLength > 12)
        {
            throw new ArgumentOutOfRangeException("Invalid company prefix");
        }

        // partition is actually arithmetic progression: https://en.wikipedia.org/wiki/Arithmetic_progression
        return (prefixLength - 13) / -1 - 1;
    }
}