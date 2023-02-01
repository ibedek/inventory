using Inventory.Application.Interfaces;

namespace Inventory.Application.Services;

public class MachineDateTime : IDateTime
{
    public DateTime Now => DateTime.Now;
}