using Sgtin.Enums;

namespace Sgtin;

public record SgtinTagInfo
{
    public TagType Type { get; set; }
    public int Filter { get; set; }
    public int Partition { get; set; }
    public ulong CompanyPrefix { get; set; }
    public ulong ItemReference { get; set; }
    public ulong Serial { get; set; }
}