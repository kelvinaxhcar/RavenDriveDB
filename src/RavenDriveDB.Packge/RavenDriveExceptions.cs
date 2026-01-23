namespace RavenDriveDB.Packge
{
    public class RavenDriveException : Exception
    {
        public RavenDriveException(string message) : base(message) { }
        public RavenDriveException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RavenDriveQuotaException : RavenDriveException
    {
        public RavenDriveQuotaException(string message) : base(message) { }
        public RavenDriveQuotaException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RavenDriveNotFoundException : RavenDriveException
    {
        public RavenDriveNotFoundException(string message) : base(message) { }
        public RavenDriveNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RavenDriveAuthException : RavenDriveException
    {
        public RavenDriveAuthException(string message) : base(message) { }
        public RavenDriveAuthException(string message, Exception innerException) : base(message, innerException) { }
    }
}
