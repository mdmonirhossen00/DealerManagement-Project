namespace DealerManagement.Model.Common
{
    public static class MessageModel
    {
        public static string InsertSuccess { get; set; } = "Saved Successfully.";
        public static string UpdateSuccess { get; set; } = "Updated Successfully.";
        public static string DeleteSuccess { get; set; } = "Data Deleted Successfully.";
        public static string InsertFail { get; set; } = "Insert Failed.";
        public static string UpdateFail { get; set; } = "Update Failed.";
        public static string DeleteFail { get; set; } = "Delete Failed.";
        public static string NotFoundForSave { get; set; } = "Data Not Found for Save.";
        public static string NotFoundForUpdate { get; set; } = "Data Not Found for Update.";
        public static string DataLoaded { get; set; } = "Data Loaded Successfully.";
        public static string DataLoadedFailed { get; set; } = "Data Not Found!";
        public static string AlreadyExists { get; set; } = "Already Exists.";
        public static string InvalidCredentials { get; set; } = "Invalid username or password.";
        public static string InactiveUser { get; set; } = "User account is inactive. Please contact administrator.";
    }
}

