namespace VitasysEHR.Utility
{
    public class SD
    {
        public const string SessionKeyPatientName = "_PatientName";
        public const string SessionKeyPatientProfPicUrl = "_PatientProfPicUrl";
        public const string SessionKeyPatientId = "_PatientId";
        public const string SessionKeySubscriptionType = "_SubscriptionType";
        public const string SessionKeySubscriptionIsLockout = "_SubscriptionIsLockout";
        public const string SessionKeyUserId = "_ClinicId";

        public const string Role_Admin = "Admin";
        public const string Role_Assistant = "Assistant";
        public const string Role_Dentist = "Dentist";
        public const string Role_Owner = "Owner";
        public const string Role_Patient = "Patient";

        public const string AuditLogin = "User Logged in";
        public const string AuditLogout = "User Logged out";
        public const string AuditLogin2Fa = "User Logged in with Two Factor Authentication";
        public const string AuditEnable2Fa = "User enabled Two Factor Authentication";
        public const string AuditDisable2Fa = "User disabled Two Factor Authentication";
        public const string AuditRegister = "New User Registered";
        public const string AuditUpdate = "Values changed";
        public const string AuditCreate = "New Value Added";
        public const string AuditArchive = "Item Archived";
        public const string AuditView = "User viewed this page";
        public const string AuditEmail = "Email Sent";
        public const string AuditDelete = "Record deleted";
        public const string AuditShare = "Document shared";


        public const string AuditForgotPassword = "Request for reset password.";
    }
}