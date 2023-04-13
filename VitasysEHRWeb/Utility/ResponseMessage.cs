namespace VitasysEHRWeb.Utility
{
    public class ResponseMessage
    {
        private const string checkIcon = "<i class=\"fas fa-check-circle text-success d-flex justify-content-center mb-3\" style=\"font-size: 100px;\"></i>";
        private const string crossIcon = "<i class=\"fas fa-times-circle text-danger d-flex justify-content-center mb-3\" style=\"font-size: 100px;\"></i>";

        public readonly static Dictionary<string, string> HEADER = new()
        {
            { "EditApproved", "Edit Request Approved" },
            { "EditDenied", "Edit Request Denied" },
            { "ArchiveApproved", "Archive Request Approved" },
            { "ArchiveDenied", "Archive Request Denied" },
            { "DeleteApproved", "Delete Request Approved" },
            { "DeleteDenied", "Delete Request Denied" },
            { "ArchiveFailed", "Archive Failed" },
            { "DeleteFailed", "Delete Failed" },
            { "Success", "Success" },
            { "UnexpectedError", "Failed" },
        };

        public readonly static Dictionary<string, string> MESSAGE = new()
        {
            { "EditApproved", "Your clinic's representative can now edit your record." },
            { "EditDenied", "Your clinic won't be able to edit your record." },
            { "ArchiveApproved", "The system has successfully archived your record." },
            { "ArchiveDenied", "Your clinic won't be able to archive your record." },
            { "DeleteApproved", "The system has successfully deleted your record." },
            { "DeleteDenied", "Your clinic won't be able to delete your record." },
            { "ArchiveFailed", "The system failed to archived your record." },
            { "DeleteFailed", "The system failed to delete your record." },
            { "Success", "Your request has been processed." },
            { "UnexpectedError", "An unexpected error happened. We were unable to complete your request." },
        };

        public readonly static Dictionary<string, string> ICON = new()
        {
            { "EditApproved", checkIcon },
            { "EditDenied", checkIcon },
            { "ArchiveApproved", checkIcon },
            { "ArchiveDenied", checkIcon },
            { "DeleteApproved", checkIcon },
            { "DeleteDenied", checkIcon },
            { "ArchiveSuccess", checkIcon },
            { "ArchiveFailed", crossIcon },
            { "DeleteFailed", crossIcon },
            { "Success", checkIcon },
            { "UnexpectedError", crossIcon },
        };
    }
}
