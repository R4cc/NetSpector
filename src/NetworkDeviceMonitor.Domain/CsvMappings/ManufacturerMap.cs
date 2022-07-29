using CsvHelper.Configuration;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.Domain.CsvMappings;

public sealed class ManufacturerMap : ClassMap<Manufacturer>
{
    public ManufacturerMap()
    {
        Map(m => m.LastUpdated).Name("Last Update");
        Map(m => m.Prefix).Name("Mac Prefix");
        Map(m => m.Name).Name("Vendor Name");
    }
}