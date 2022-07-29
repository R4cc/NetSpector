using System.Collections;
using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.Domain.CsvMappings;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Services;

public class ManufacturerDataService
{
    private readonly string _downloadUrl;
    private readonly string _filePath;
    private IUnitOfWork _uow;
    private readonly HttpClient _httpClient = new();
    
    public ManufacturerDataService(IConfiguration config, IUnitOfWork uow)
    {
        _downloadUrl = config.GetValue<string>("MacVendorList:DownloadUrl");
        _filePath = config.GetValue<string>("MacVendorList:LocalCsvFilePath");
        _uow = uow;
    }

    // Loads all MAC Vendors from a CSV File in a model list and saves / updates all to db
    public async Task RefreshManufacturerData()
    {
        await DownloadFile(_downloadUrl,_filePath);
        var csvManufacturerList = (await GetManufacturersFromCsvFile(_filePath));
        
        var manufacturersList = (await _uow.IManufacturerRepository.GetAll())
            .ToDictionary(item => item.Prefix, item => item);
        
        var manufacturersToAdd = new List<Manufacturer>();
        var manufacturersToUpdate = new List<Manufacturer>();
        

        foreach (var manufacturer in csvManufacturerList)
        {
            // Entry does not exist in DB, so add it
            if (!manufacturersList.ContainsKey(manufacturer.Prefix))
            {
                manufacturersToAdd.Add(manufacturer);
                continue;
            }
            
            // Object exists; Update the values if entry was updated
            if(manufacturersList[manufacturer.Prefix].LastUpdated > manufacturer.LastUpdated )
            {
                manufacturersToUpdate.Add(manufacturer);
            }
        }

        // bulk create entities
        await _uow.IManufacturerRepository.BulkCreate(manufacturersToAdd);
        await _uow.IManufacturerRepository.BulkUpdate(manufacturersToUpdate);
        
        // Delete once everything is finished
        await DeleteFile(_filePath);
    }

    private async Task<List<Manufacturer>> GetManufacturersFromCsvFile(string path)
    {
        List<Manufacturer> records;

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ManufacturerMap>();
        records = csv.GetRecords<Manufacturer>().ToList();

        return records;
    }

    private async Task DownloadFile(string downloadUrl, string filePath)
    {
        // If it exists, delete it
        await DeleteFile(filePath);

        if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("URI is invalid.");
        }

        byte[] fileBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(filePath, fileBytes);
    }

    private async Task DeleteFile(string filePath)
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}