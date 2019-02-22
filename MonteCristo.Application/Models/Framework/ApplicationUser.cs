using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MonteCristo.Application.Models.Framework
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [BsonIgnoreExtraElements]
    public class ApplicationUser : MongoUser
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public string Avatar { get; set; }
        public string FacebookAddress { get; set; }
        public string TwitterAddress { get; set; }
        public string InstagramAddress { get; set; }

        public DateTime? Expired { get; set; }

        public bool Locked { get; set; }
        //Tổng số tiền user đã nạp vào hệ thống
        public int Total { get; set; }
    }
}
