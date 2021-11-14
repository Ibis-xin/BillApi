using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillApi.Models;
using BillApi.MongoCollection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BillApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InjectFakeDataController : ControllerBase
    {
        private readonly ILogger<InjectFakeDataController> _logger;
        private readonly IRecord _collectionBill;

        public InjectFakeDataController(ILogger<InjectFakeDataController> logger, IRecord collectionBill)
        {
            _logger = logger;
            _collectionBill = collectionBill;
        }
        private readonly List<Project> _fakeProject = new()
        {
            new Project()
            {
                Name = "MacBook Air",
                Remark = "silver, 8GB/256GB, 7 core",
                Quantity = 1,
                UnitPrice = 30900

            },
            new Project()
            {
                Name = "MacBook Air",
                Remark = "silver, 16GB/2T, 7 core",
                Quantity = 1,
                UnitPrice = 60900
            },
            new Project()
            {
                Name = "MacBook Air",
                Remark = "space gray, 8GB/256GB, 8 core",
                Quantity = 1,
                UnitPrice = 38900
            },
            new Project()
            {
                Name = "MacBook Air",
                Remark = "silver, 16GB/2T, 7 core",
                Quantity = 1,
                UnitPrice = 60900
            },
            new Project()
            {
                Name = "MacBook Pro",
                Remark = "gold, 64GB/8T, 32 core",
                Quantity = 1,
                UnitPrice = 182900
            },
            new Project()
            {
                Name = "MacBook Pro",
                Remark = "silver, 64GB/8T, 16 core",
                Quantity = 1,
                UnitPrice = 146900
            },
            new Project()
            {
                Name = "MacBook Pro",
                Remark = "gold, 16GB/512GB, 16 core",
                Quantity = 1,
                UnitPrice = 74900
            },
            new Project()
            {
                Name = "MacBook Pro",
                Remark = "silver, 16GB/512GB, 16 core",
                Quantity = 1,
                UnitPrice = 74900
            },
        };
        private readonly List<string> _fakePurchaser = new()
        {
            "Abe",
            "Abel",
            "Abner",
            "Abraham",
            "Allen",
            "Adam",
            "Adelaide",
            "Aileen",
            "Alex",
            "Alexandra",
            "Alexis",
            "Alice",
            "Alison",
            "Amanda",
            "Amy",
            "Angela",
            "Angie"
        };
        private readonly List<string>  _fakeOther = new()
        {
            "Please deliver on Monday",
            "Please help me remove the outer box",
            "Please give me uniform coding",
            "I don't need an invoice",
            "Please deposit my invoice in the vehicle"
        };

        [HttpPost]
        public async Task<ActionResult> InjectFakeData(int round = 100)
        {
            try
            {
                var fakeBills = new List<Bill>();
                var random = new Random(Guid.NewGuid().GetHashCode());

                for (var i = 0; i < round; i++)
                {
                    var month = i % 12 + 1;
                    var day = i % 27 + 1;
                    var projects = new List<Project>();
                    for (var j = 0; j < random.Next(1, 8); j++)
                    {
                        projects.Add(_fakeProject[random.Next(0, _fakeProject.Count - 1)]);
                    }

                    fakeBills.Add(
                        new Bill
                        {
                            Purchaser = _fakePurchaser[random.Next(0, _fakePurchaser.Count - 1)],
                            Phone = $"09-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}",
                            Other = _fakeOther[random.Next(0, _fakeOther.Count - 1)],
                            CheckOutDateTime = new DateTime(2021, month, day),
                            CreationDateTime = new DateTime(2021, month, day + i % 4),
                            ProjectList = projects
                        });
                }

                await _collectionBill.InsretManyBill(fakeBills);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
