using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillApi.Models;
using BillApi.MongoCollection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BillApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RecordController : ControllerBase
    {
        private readonly ILogger<RecordController> _logger;
        private readonly IRecord _collectionBill;

        public RecordController(ILogger<RecordController> logger, IRecord collectionBill)
        {
            _logger = logger;
            _collectionBill = collectionBill;
        }

        [HttpGet("GetBills")]
        public async Task<ActionResult<List<Bill>>> GetBills(string startId = "", int count = 10)
        {
            try
            {
                var filter
                    = startId != string.Empty
                        ? Builders<Bill>.Filter.Eq(i => i.Id, startId)
                        : Builders<Bill>.Filter.Empty;

                var result = await _collectionBill.GetBills(filter, count);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new MessageTemplate { BadRequestError = e.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Bill>>> SearchBillWithPhone(string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return BadRequest(new MessageTemplate { BadRequestError = "phone isn't null or empty." });

                var result = await _collectionBill.SearchBillWithPhone(phone);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new MessageTemplate { BadRequestError = e.Message });
            }
        }

        [HttpPost("insertBill")]
        public async Task<ActionResult<Bill>> InsertBill(BillDetail bill)
        {
            try
            {
                var messageTemplate = new MessageTemplate();

                if (string.IsNullOrEmpty(bill.Purchaser))
                    messageTemplate.BadRequestError = $"Data '{nameof(BillDetail.Purchaser)}' not exist.";

                if (string.IsNullOrEmpty(bill.Phone))
                    messageTemplate.BadRequestError = $"Data '{nameof(BillDetail.Phone)}' not exist.";

                if (bill.ProjectList == null || bill.ProjectList.Count == 0)
                    messageTemplate.BadRequestError = $"Data '{nameof(BillDetail.ProjectList)}' error.";

                if (!string.IsNullOrEmpty(messageTemplate.BadRequestError))
                    return BadRequest(messageTemplate);

                var document = new Bill(bill) { CreationDateTime = DateTime.UtcNow };
                await _collectionBill.InsertBill(document);
                return Ok(document);
            }
            catch (Exception e)
            {
                return BadRequest(new MessageTemplate { BadRequestError = e.Message });
            }
        }

        [HttpPut("updateCheckOutTime")]
        public async Task<ActionResult<Bill>> UpdateCheckOutTime(string id, DateTime checkOutTime)
        {
            try
            {
                var target = await _collectionBill.CheckBillExist(id);

                var messageTemplate = new MessageTemplate();
                if (target == null)
                    messageTemplate.BadRequestError = $"ObjectId: {id} not exist.";
                if (target!.CheckOutDateTime != null)
                    messageTemplate.BadRequestError = $"ObjectId: {id}, { nameof(Bill.CheckOutDateTime) } isn't null.";

                if (!string.IsNullOrEmpty(messageTemplate.BadRequestError))
                    return BadRequest(messageTemplate);

                return Ok(); //TODO 缺一個更新動作
            }
            catch (Exception e)
            {
                return BadRequest(new MessageTemplate { BadRequestError = e.Message });
            }
        }


        [HttpDelete("deleteBill")]
        public async Task<ActionResult<MessageTemplate>> DeleteBill(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return BadRequest();

                var result = await _collectionBill.DeleteBill(id);

                return result.IsAcknowledged
                    ? Ok(new MessageTemplate { Message = "Delete success." })
                    : BadRequest(new MessageTemplate { BadRequestError = $"ObjectId: {id} not exist." });//TODO 確認IsAcknowledged的意思
            }
            catch (Exception e)
            {
                return BadRequest(new MessageTemplate { BadRequestError = e.Message });
            }
        }
    }
}
