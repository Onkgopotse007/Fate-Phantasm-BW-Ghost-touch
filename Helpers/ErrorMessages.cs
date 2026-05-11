namespace RPG_dotnet.Helpers
{
    public static class ErrorMessages
    {
        public const string VALIDATION_ERROR = "One or more validation errors occurred.";
        public const string EXTERNAL_API_ERROR = "An external service responded with an error.";
        public const string CONFLICT_ERROR = "A conflict occurred (e.g., duplicate entry).";
        public const string DATABASE_ERROR = "A database error occurred.";
        public const string INTERNAL_SERVER_ERROR = "An internal server error occurred.";
        public const string NOT_FOUND_ERROR = "The requested resource was not found.";
        public const string AUTH_ERROR = "Authentication or authorization failed.";
        public const string REQUEST_ERROR = "An unknown error occurred.";
        public const string ORM_ERROR = "An ORM error occurred.";
    }
}