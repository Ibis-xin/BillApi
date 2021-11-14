using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillApi.Models
{
    
    public class Bill : BillDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime? CheckOutDateTime { get; set; }

        public Bill(BillDetail detail = null)
        {
            Purchaser = detail?.Purchaser;
            Phone = detail?.Phone;
            Other = detail?.Other;
            ProjectList = detail?.ProjectList;
        }
    }

    public class BillDetail
    {
        [Required]
        public string Purchaser { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public DateTime CreationDateTime { get; set; }
        [Required]
        public List<Project> ProjectList { get; set; }
        [Required]
        public string Other { get; set; }
    }
}




