using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Text;

namespace JetBlack.HttpServer
{
    public class HttpRequest
    {
        public HttpListenerRequest Request { get; }

        public List<string> AcceptTypes { get; set; }
        public Encoding ContentEncoding { get; set; }
        public long ContentLength { get; set; }
        public string? ContentType { get; set; }
        public CookieCollection Cookies => Request.Cookies;
        public bool HasEntityBody { get; set; }
        public NameValueCollection Headers => Request.Headers;
        public string Method => Request.HttpMethod;
        public Stream Body { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsLocal { get; set; }
        public bool IsSecureConnection { get; set; }
        public bool IsWebSocketRequest { get; set; }
        public bool KeepAlive { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public Version ProtocolVersion { get; set; }
        public NameValueCollection QueryString => Request.QueryString;
        public string? RawUrl { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public Guid RequestTraceIdentifier { get; set; }
        public string? ServiceName { get; set; }
        public TransportContext TransportContext => Request.TransportContext;
        public Uri? Url { get; set; }
        public Uri? UrlReferrer { get; set; }
        public string UserAgent { get; set; }
        public string UserHostAddress { get; set; }
        public string UserHostName { get; set; }
        public List<string> UserLanguages { get; set; }

        public HttpRequest(HttpListenerRequest req)
        {
            Request = req;
            AcceptTypes = req.AcceptTypes == null ? new List<string>() : new List<string>(req.AcceptTypes);
            ContentEncoding = req.ContentEncoding;
            ContentLength = req.ContentLength64;
            ContentType = req.ContentType;
            HasEntityBody = req.HasEntityBody;
            Body = req.InputStream;
            IsAuthenticated = req.IsAuthenticated;
            IsLocal = req.IsLocal;
            IsSecureConnection = req.IsSecureConnection;
            IsWebSocketRequest = req.IsWebSocketRequest;
            KeepAlive = req.KeepAlive;
            LocalEndPoint = req.LocalEndPoint;
            ProtocolVersion = req.ProtocolVersion;
            RawUrl = req.RawUrl;
            RemoteEndPoint = req.RemoteEndPoint;
            RequestTraceIdentifier = req.RequestTraceIdentifier;
            ServiceName = req.ServiceName;
            Url = req.Url;
            UrlReferrer = req.UrlReferrer;
            UserAgent = req.UserAgent;
            UserHostAddress = req.UserHostAddress;
            UserHostName = req.UserHostName;
            UserLanguages = req.UserLanguages == null ? new List<string>() : new List<string>(req.UserLanguages);
        }
    }
}