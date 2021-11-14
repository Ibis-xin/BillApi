using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillApi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillApi.MongoCollection
{
    public class Record : IRecord
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Bill> _mongoDbBill;
        
        private const string DataBaseName = "Order";
        private const string CollectionName = "ConsumerDetails";
        public Record(IConfiguration configuration)
        {
            _configuration = configuration;

            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(
                    _configuration.GetSection("MongoDB:Url").Value,
                    int.Parse(_configuration.GetSection("MongoDB:Port").Value))
            };

            _mongoDbBill = new MongoClient(settings)
                .GetDatabase(DataBaseName)
                .GetCollection<Bill>(CollectionName);
        }

        public Task<List<Bill>> GetBills(FilterDefinition<Bill> filter, int count) =>
            _mongoDbBill.Find(filter)
                .SortBy(x => x.CreationDateTime)
                .Limit(count)
                .ToListAsync();

        public Task<List<Bill>> SearchBillWithPhone(string phone) =>
            _mongoDbBill
                .Find(x => x.Phone == phone)
                .ToListAsync();

        public Task InsertBill(Bill document) => _mongoDbBill.InsertOneAsync(document);

        public Task InsretManyBill(IEnumerable<Bill> bills) => _mongoDbBill.InsertManyAsync(bills);

        public Task<DeleteResult> DeleteBill(string id) =>
            _mongoDbBill.DeleteOneAsync(
                x => x.Id == id);

        public Task<Bill> CheckBillExist(string id) =>
            _mongoDbBill
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
    }

    public interface IRecord
    {
        Task<List<Bill>> GetBills(FilterDefinition<Bill> filter, int count);
        Task<List<Bill>> SearchBillWithPhone(string phone);
        Task InsertBill(Bill document);
        Task InsretManyBill(IEnumerable<Bill> bills);
        Task<DeleteResult> DeleteBill(string id);
        Task<Bill> CheckBillExist(string id);
    }
}
