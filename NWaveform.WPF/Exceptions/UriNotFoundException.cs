using System;
using System.Globalization;
using System.Net;

namespace NWaveform.Exceptions
{
    public sealed class UriNotFoundException : AudioException
    {
        public Uri Uri { get; }
        public HttpStatusCode StatusCode { get; }

        public UriNotFoundException(Uri uri, HttpStatusCode statusCode) :
            base(string.Format(CultureInfo.CurrentCulture, "Could not get uri \"{0}\" (HTTP status: {1})", uri, statusCode))
        {
            Uri = uri;
            StatusCode = statusCode;
        }
    }
}
