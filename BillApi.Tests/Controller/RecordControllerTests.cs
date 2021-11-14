using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillApi.Controllers;
using BillApi.Models;
using BillApi.MongoCollection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace BillApi.Tests.Controller
{
    [TestFixture]
    class RecordControllerTests
    {
        private IRecord _collectionRecord;
        private RecordController _recordController;

        [SetUp]
        public void Setup()
        {
            _collectionRecord = Substitute.For<IRecord>();
            _recordController = new RecordController(new NullLogger<RecordController>(), _collectionRecord);
        }

        [Test]
        public void RecordController_HasAuthorizeAttribute()
        {
            Assert.That(typeof(RecordController), Has.Attribute<AuthorizeAttribute>());
        }


        [Test]
        public async Task GetBills_WhenIdIsCorrectAndCountIsSpecified_ReturnsOkStatusAndCheckReturnData()
        {
            const string phone = "09-1111-1111";
            const string startId = "keyId";
            const int count = 1;

            _collectionRecord
                .GetBills(Arg.Any<FilterDefinition<Bill>>(), count)
                .Returns(new List<Bill> { new() { Phone = phone } });

            var result = (ObjectResult)(await _recordController.GetBills(startId, count)).Result;

            await _collectionRecord.Received(1).GetBills(Arg.Any<FilterDefinition<Bill>>(), count);
            Assert.That(((List<Bill>)result.Value)[0].Phone, Is.EqualTo(phone));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetBills_WhenIdIsCorrectAndCountIsNotSpecified_ReturnsOkStatusAndCountPresetTen()
        {
            const string phone = "09-1111-1111";
            const string startId = "keyId";
            const int presetCount = 10;
            _collectionRecord
                .GetBills(Arg.Any<FilterDefinition<Bill>>(), presetCount)
                .Returns(new List<Bill> { new() { Phone = phone } });

            var result = (ObjectResult)(await _recordController.GetBills(startId)).Result;

            await _collectionRecord.Received(1).GetBills(Arg.Any<FilterDefinition<Bill>>(), presetCount);
            Assert.That(((List<Bill>)result.Value)[0].Phone, Is.EqualTo(phone));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetBills_WhenIdIsEmpty_ReturnsOkStatusAndCheckReturnFilterEmptyData()
        {
            const string phone = "09-1111-1111";
            _collectionRecord
                .GetBills(Builders<Bill>.Filter.Empty, Arg.Any<int>())
                .Returns(new List<Bill> { new() { Phone = phone } });

            var result = (ObjectResult)(await _recordController.GetBills(string.Empty)).Result;

            await _collectionRecord.Received(1).GetBills(Builders<Bill>.Filter.Empty, Arg.Any<int>());
            Assert.That(((List<Bill>)result.Value)[0].Phone, Is.EqualTo(phone));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetBills_WhenThrow_ReturnsBadRequestStatusAndCheckReturnData()
        {
            const string startId = "keyId";
            _collectionRecord
                .GetBills(Arg.Any<FilterDefinition<Bill>>(), Arg.Any<int>())
                .Throws<Exception>();

            var result = (ObjectResult)(await _recordController.GetBills(startId)).Result;

            await _collectionRecord.Received(1).GetBills(Arg.Any<FilterDefinition<Bill>>(), Arg.Any<int>());
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(((MessageTemplate)result.Value).BadRequestError, Is.EqualTo("Exception of type 'System.Exception' was thrown."));
        }
    }
}
