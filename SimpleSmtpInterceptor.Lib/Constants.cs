namespace SimpleSmtpInterceptor.Lib
{
    public static class Headers
    {
        public const string Subject = "Subject: ";
        public const string From = "From: ";
        public const string To = "To: ";
        public const string MimeVersion = "MIME-Version: ";
        public const string Date = "Date: ";
        public const string ContentType = "Content-Type: ";
        public const string ContentTransferEncoding = "Content-Transfer-Encoding: ";
        public const string ContentDisposition = "Content-Disposition: ";
        public const string Boundary = " boundary=";
    }

    public static class ContentDispositions
    {
        public const string Attachment = "attachment";
    }

    public static class ContentFileTypes
    {
        public const string TextFile = "text/plain; name=";
        public const string MultiPartMixed = "multipart/mixed;";
    }

    public static class ContentTypeBodies
    {
        public const string TextOrHtml = "text/html; charset=us-ascii";
    }

    public static class ContentTransferEncodings
    {
        public const string Base64 = "base64";
        public const string QuotedPrintable = "quoted-printable";
    }
}
