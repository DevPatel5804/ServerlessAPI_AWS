using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace serverless_auth.ViewModels
{
    [DynamoDBTable("auth-users")]
    public class UserViewModel
    {
        [DynamoDBHashKey("ApplicationID")]
        public string ApplicationID { get; set; }

        [DynamoDBRangeKey("UserID")]
        public string UserID { get; set; }

        [DynamoDBProperty("PasswordHash")] // This property stores the hashed password
        public string PasswordHash { get; set; }

        [DynamoDBProperty("RefreshToken")]
        public string RefreshToken { get; set; }

        [DynamoDBProperty("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [DynamoDBProperty("ModifiedOn")]
        public DateTime ModifiedOn { get; set; }

        [DynamoDBProperty("IsActive")]
        public bool? IsActive { get; set; }

        [DynamoDBProperty("IsLocked")]
        public bool IsLocked { get; set; }

        [DynamoDBProperty("IsEnabled")]
        public bool? IsEnabled { get; set; }

        // --- Changed to DateTime? for ISO 8601 string storage (and removed Unix wrappers) ---
        [DynamoDBProperty("LockedOn")]
        public DateTime? LockedOn { get; set; }

        [DynamoDBProperty("LastLoggedOn")]
        public DateTime? LastLoggedOn { get; set; }

        [DynamoDBProperty("FailedAttemptOn")]
        public DateTime? FailedAttemptOn { get; set; }

        [DynamoDBProperty("FailedLoginAttempts")]
        public int FailedLoginAttempts { get; set; }
    }
}

