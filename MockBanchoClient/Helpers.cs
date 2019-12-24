using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Amib.Threading;
namespace MockBanchoClient.Helpers {
    public class pWebRequest : IDisposable {
        private string url;

        public bool Aborted { get; private set; }

        public bool Completed { get; private set; }

        public bool KeepEventsBound;

        public Dictionary<string, string> Parameters = new Dictionary<string, string> ();

        public Dictionary<string, byte[]> Files = new Dictionary<string, byte[]> ();

        public Dictionary<string, string> Headers = new Dictionary<string, string> ();

        public int Timeout = 16384;

        // private static Logger logger;

        private static SmartThreadPool threadPool;

        private IWorkItemResult workItem;

        private string address;

        private HttpWebRequest request;

        private HttpWebResponse response;

        private Stream internalResponseStream;

        private static AddressFamily? preferredNetwork;

        public static bool UseExplicitIPv4Requests;

        private static bool? useFallbackPath;

        private bool didGetIPv6IP;

        private int responseBytesRead;

        private byte[] buffer;

        private readonly MemoryStream requestBody = new MemoryStream ();

        public Stream ResponseStream;

        private long lastAction;

        private bool isDisposed;

        private int retryCount = 2;

        private int retriesRemaining = 2;

        private bool hasExceededTimeout {
            get {
                return this.timeSinceLastAction > (long) this.Timeout;
            }
        }

        public byte[] ResponseData {
            get {
                byte[] array;
                try {
                    MemoryStream memoryStream = new MemoryStream ((int) this.ResponseStream.Length);
                    try {
                        this.ResponseStream.Seek (0L, SeekOrigin.Begin);
                        this.ResponseStream.CopyTo (memoryStream);
                        array = memoryStream.ToArray ();
                    } finally {
                        ((IDisposable) memoryStream).Dispose ();
                    }
                } catch {
                    array = null;
                }
                return array;
            }
        }

        public WebHeaderCollection ResponseHeaders {
            get {
                HttpWebResponse httpWebResponse = this.response;
                if (httpWebResponse != null) {
                    return httpWebResponse.Headers;
                }
                return null;
            }
        }

        public string ResponseString {
            get {
                string end;
                try {
                    this.ResponseStream.Seek (0L, SeekOrigin.Begin);
                    end = (new StreamReader (this.ResponseStream, Encoding.UTF8)).ReadToEnd ();
                } catch {
                    end = null;
                }
                return end;
            }
        }

        public int RetryCount {
            get {
                return this.retryCount;
            }
            set {
                int num = value;
                int num1 = num;
                this.retryCount = num;
                this.retriesRemaining = num1;
            }
        }

        private long timeSinceLastAction {
            get {
                return (DateTime.Now.Ticks - this.lastAction) / 10000L;
            }
        }

        public string Url {
            get {
                return this.url;
            }
            private set {
                if (!value.StartsWith ("https://")) {
                    value = string.Concat ("https://", value.Replace ("http://", string.Empty));
                }
                this.url = value;
            }
        }

        static pWebRequest () {
            pWebRequest.preferredNetwork = new AddressFamily?(AddressFamily.InterNetwork);
            pWebRequest.UseExplicitIPv4Requests = false;
            pWebRequest.threadPool = new SmartThreadPool (new STPStartInfo () {
                MaxWorkerThreads = 64,
                    AreThreadsBackground = true,
                    IdleTimeout = 300000
            });
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 12;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            // pWebRequest.logger = Logger.GetLogger(LoggingTarget.Network, true);
        }

        public pWebRequest (string url, params object[] args) {
            this.Url = (args.Length == 0 ? url : string.Format (url, args));
        }

        public void Abort () {
            if (this.Aborted) {
                return;
            }
            this.Aborted = true;
            this.abortRequest ();
            IWorkItemResult workItemResult = this.workItem;
            if (workItemResult != null) {
                workItemResult.Cancel ();
            } else { }
            this.workItem = null;
            if (!this.KeepEventsBound) {
                this.unbindEvents ();
            }
        }

        private void abortRequest () {
            try {
                HttpWebRequest httpWebRequest = this.request;
                if (httpWebRequest != null) {
                    httpWebRequest.Abort ();
                } else { }
                HttpWebResponse httpWebResponse = this.response;
                if (httpWebResponse != null) {
                    httpWebResponse.Close ();
                } else { }
            } catch { }
        }

        public void AddFile (string name, byte[] data) {
            this.Files.Add (name, data);
        }

        public void AddHeader (string key, string val) {
            this.Headers.Add (key, val);
        }

        public void AddParameter (string name, string value) {
            this.Parameters.Add (name, value);
        }

        public void AddRaw (Stream stream) {
            stream.CopyTo (this.requestBody);
        }

        public void AddRaw (byte[] data) {
            this.requestBody.Write (data, 0, (int) data.Length);
        }

        private void beginRequestOutput () {
            try {
                using (Stream requestStream = this.request.GetRequestStream ()) {
                    this.reportForwardProgress ();
                    this.requestBody.Position = 0L;
                    byte[] numArray = new byte[32768];
                    int num = 0;
                    while (true) {
                        int num1 = this.requestBody.Read (numArray, 0, 32768);
                        int num2 = num1;
                        if (num1 <= 0) {
                            break;
                        }
                        this.reportForwardProgress ();
                        requestStream.Write (numArray, 0, num2);
                        requestStream.Flush ();
                        num += num2;
                        pWebRequest.RequestUpdateHandler requestUpdateHandler = this.UploadProgress;
                        if (requestUpdateHandler != null) {
                            requestUpdateHandler (this, (long) num, this.request.ContentLength);
                        } else { }
                    }
                }
                this.beginResponse ();
            } catch (Exception exception) {
                this.Complete (exception);
            }
        }

        private void beginResponse () {
            try {
                this.response = this.request.GetResponse () as HttpWebResponse;
                pWebRequest.RequestStartedHandler requestStartedHandler = this.Started;
                if (requestStartedHandler != null) {
                    requestStartedHandler (this);
                } else { }
                this.internalResponseStream = this.response.GetResponseStream ();
                this.checkCertificate ();
                this.buffer = new byte[32768];
                this.reportForwardProgress ();
                while (true) {
                    int num = this.internalResponseStream.Read (this.buffer, 0, 32768);
                    this.reportForwardProgress ();
                    if (num <= 0) {
                        break;
                    }
                    this.ResponseStream.Write (this.buffer, 0, num);
                    this.responseBytesRead += num;
                    pWebRequest.RequestUpdateHandler requestUpdateHandler = this.DownloadProgress;
                    if (requestUpdateHandler != null) {
                        requestUpdateHandler (this, (long) this.responseBytesRead, this.response.ContentLength);
                    } else { }
                }
                this.ResponseStream.Seek (0L, SeekOrigin.Begin);
                this.Complete (null);
            } catch (Exception exception) {
                this.Complete (exception);
            }
        }

        private IPEndPoint bindEndPoint (ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount) {
            this.didGetIPv6IP = this.didGetIPv6IP | remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6;
            return null;
        }

        public virtual void BlockingPerform () {
            Exception exc = null;
            bool completed = false;
            this.Finished += (request, e) => {
                exc = e;
                completed = true;
            };
            this.perform ();
            while (!completed && !this.Aborted) {
                Thread.Sleep (10);
            }
            if (exc != null) {
                throw exc;
            }
        }

        // static Stream cert_stream = null;
        private void checkCertificate () {
            // object[] objArray = new object[] { this };
            // Secret.aa78dc549ab5a479b9d1aacf1c67b8fe3 ().fZymResnG7xXI (
            //     Secret.GetCertStream (), "IsufnI!X+`", objArray
            // );
        }

        private object checkTimeoutLoop (object state) {
            while (!this.Aborted && !this.Completed) {
                if (this.hasExceededTimeout) {
                    this.abortRequest ();
                }
                Thread.Sleep (500);
            }
            return state;
        }

        protected virtual void Complete (Exception exception_0) {
            HttpStatusCode statusCode;
            if (this.Aborted) {
                return;
            }
            WebException exception0 = exception_0 as WebException;
            if (exception0 != null) {
                bool flag = true;
                HttpWebResponse response = exception0.Response as HttpWebResponse;
                if (response != null) {
                    statusCode = response.StatusCode;
                } else {
                    statusCode = HttpStatusCode.RequestTimeout;
                }
                HttpStatusCode? nullable = new HttpStatusCode?(statusCode);
                if (nullable.HasValue) {
                    HttpStatusCode valueOrDefault = nullable.GetValueOrDefault ();
                    if ((int) valueOrDefault - (int) HttpStatusCode.Forbidden <= 2) {
                        flag = false;
                    } else if (valueOrDefault == HttpStatusCode.RequestTimeout && this.hasExceededTimeout) {
                        WebException webException = new WebException (
                            string.Format (
                                "Timeout to {0} ({1}) after {2} seconds idle (read {3} bytes).",
                                new object[] { this.Url, this.address, this.timeSinceLastAction / 1000L, this.responseBytesRead }
                            ), WebExceptionStatus.Timeout
                        );
                        exception0 = webException;
                        exception_0 = webException;
                    }
                }
                if (flag) {
                    int num = this.retriesRemaining;
                    this.retriesRemaining = num - 1;
                    if (num > 0 && this.responseBytesRead == 0) {
                        // pWebRequest.logger.Add(string.Format("Request to {0} ({1}) failed with {2} (retrying {3}/{4}).", new object[] { this.Url, this.address, nullable, 2 - this.retriesRemaining, 2 }), 1);
                        this.perform ();
                        return;
                    }
                }
                if (!pWebRequest.useFallbackPath.HasValue & flag && this.didGetIPv6IP) {
                    pWebRequest.useFallbackPath = new bool?(true);
                    // pWebRequest.logger.Add("---------------------- USING FALLBACK PATH! ---------------------", 1);
                }
                // pWebRequest.logger.Add(string.Format("Request to {0} ({1}) failed with {2} (FAILED).", this.Url, this.address, nullable), 1);
            } else if (exception_0 == null) {
                if (!pWebRequest.useFallbackPath.HasValue) {
                    pWebRequest.useFallbackPath = new bool?(false);
                }
                // pWebRequest.logger.Add(string.Concat(new string[] { "Request to ", this.Url, " (", this.address, ") successfully completed!" }), 1);
            }
            HttpWebResponse httpWebResponse = this.response;
            if (httpWebResponse != null) {
                httpWebResponse.Close ();
            } else { }
            pWebRequest.RequestCompleteHandler requestCompleteHandler = this.Finished;
            if (requestCompleteHandler != null) {
                requestCompleteHandler (this, exception_0);
            } else { }
            if (!this.KeepEventsBound) {
                this.unbindEvents ();
            }
            if (exception_0 != null) {
                this.Aborted = true;
                return;
            }
            this.Completed = true;
        }

        protected virtual Stream CreateOutputStream () {
            return new MemoryStream ();
        }

        protected virtual HttpWebRequest CreateWebRequest () {
            HttpWebRequest valueOrDefault;
            bool? nullable = pWebRequest.useFallbackPath;
            if (nullable.GetValueOrDefault () & nullable.HasValue || pWebRequest.UseExplicitIPv4Requests) {
                string str = this.Url.Split (new char[] { '/', ':' }) [3];
                this.address = null;
                IPAddress[] hostAddresses = Dns.GetHostAddresses (str);
                for (int i = 0; i < (int) hostAddresses.Length; i++) {
                    IPAddress pAddress = hostAddresses[i];
                    AddressFamily addressFamily = pAddress.AddressFamily;
                    AddressFamily? nullable1 = pWebRequest.preferredNetwork;
                    bool flag = addressFamily == nullable1.GetValueOrDefault () & nullable1.HasValue;
                    bool flag1 = flag;
                    if (flag || this.address == null) {
                        AddressFamily addressFamily1 = pAddress.AddressFamily;
                        if (addressFamily1 == AddressFamily.InterNetwork) {
                            this.address = string.Format ("{0}", pAddress);
                        } else if (addressFamily1 == AddressFamily.InterNetworkV6) {
                            this.address = string.Format ("[{0}]", pAddress);
                        }
                        if (flag1) {
                            break;
                        }
                    }
                }
                valueOrDefault = WebRequest.Create (this.Url.Replace (str, string.Concat (this.address, ":443"))) as HttpWebRequest;
            } else {
                valueOrDefault = WebRequest.Create (this.Url) as HttpWebRequest;
                ServicePoint servicePoint = valueOrDefault.ServicePoint;
                servicePoint.BindIPEndPointDelegate = (BindIPEndPoint) Delegate.Combine (servicePoint.BindIPEndPointDelegate, new BindIPEndPoint (this.bindEndPoint));
            }
            valueOrDefault.UserAgent = "osu!";
            nullable = pWebRequest.useFallbackPath;
            valueOrDefault.KeepAlive = !(nullable.GetValueOrDefault () & nullable.HasValue);
            valueOrDefault.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            valueOrDefault.Host = this.Url.Split (new char[] { '/' }) [2];
            valueOrDefault.ReadWriteTimeout = -1;
            valueOrDefault.Timeout = -1;
            return valueOrDefault;
        }

        protected void Dispose (bool disposing) {
            if (this.isDisposed) {
                return;
            }
            this.isDisposed = true;
            if (!(this.ResponseStream is MemoryStream)) {
                Stream responseStream = this.ResponseStream;
                if (responseStream != null) {
                    responseStream.Dispose ();
                } else { }
            }
            Stream stream = this.internalResponseStream;
            if (stream != null) {
                stream.Dispose ();
            } else { }
            HttpWebResponse httpWebResponse = this.response;
            if (httpWebResponse != null) {
                httpWebResponse.Close ();
            } else { }
            this.unbindEvents ();
        }

        public void Dispose () {
            this.Dispose (true);
            GC.SuppressFinalize (this);
        }

        ~pWebRequest () {
            this.Dispose (false);
        }

        private void perform () {
            this.Aborted = false;
            this.abortRequest ();
            try {
                this.reportForwardProgress ();
                pWebRequest.threadPool.QueueWorkItem (new WorkItemCallback (this.checkTimeoutLoop));
                this.request = this.CreateWebRequest ();
                this.ResponseStream = this.CreateOutputStream ();
                foreach (KeyValuePair<string, string> header in this.Headers) {
                    this.request.Headers.Add (header.Key, header.Value);
                }
                if (this.Parameters.Count + this.Files.Count > 0) {
                    this.request.ContentType = "multipart/form-data; boundary=-----------------------------28947758029299";
                    foreach (KeyValuePair<string, string> parameter in this.Parameters) {
                        this.requestBody.WriteLine ("-------------------------------28947758029299");
                        this.requestBody.WriteLine (string.Concat ("Content-Disposition: form-data; name=\"", parameter.Key, "\""));
                        this.requestBody.WriteLine (string.Empty);
                        this.requestBody.WriteLine (parameter.Value);
                    }
                    foreach (KeyValuePair<string, byte[]> file in this.Files) {
                        this.requestBody.WriteLine ("-------------------------------28947758029299");
                        this.requestBody.WriteLine (string.Concat (new string[] { "Content-Disposition: form-data; name=\"", file.Key, "\"; filename=\"", file.Key, "\"" }));
                        this.requestBody.WriteLine ("Content-Type: application/octet-stream");
                        this.requestBody.WriteLine (string.Empty);
                        this.requestBody.Write (file.Value, 0, (int) file.Value.Length);
                        this.requestBody.WriteLine (string.Empty);
                    }
                    this.requestBody.WriteLine ("-------------------------------28947758029299--");
                    this.requestBody.Flush ();
                }
                if (this.requestBody.Length == 0) {
                    this.beginResponse ();
                } else {
                    this.request.Method = "POST";
                    this.request.ContentLength = this.requestBody.Length;
                    pWebRequest.RequestUpdateHandler requestUpdateHandler = this.UploadProgress;
                    if (requestUpdateHandler != null) {
                        requestUpdateHandler (this, 0L, this.request.ContentLength);
                    } else { }
                    this.reportForwardProgress ();
                    this.beginRequestOutput ();
                }
            } catch (Exception exception) {
                this.Complete (exception);
            }
        }

        public void Perform () {
            this.workItem = pWebRequest.threadPool.QueueWorkItem (new Amib.Threading.Action (this.perform));
            if (pWebRequest.threadPool.InUseThreads == pWebRequest.threadPool.MaxThreads) {
                // pWebRequest.logger.Add("WARNING: ThreadPool is saturated!", 3);
            }
        }

        private void reportForwardProgress () {
            this.lastAction = DateTime.Now.Ticks;
        }

        private void unbindEvents () {
            this.UploadProgress = null;
            this.DownloadProgress = null;
            this.Finished = null;
            this.Started = null;
            try {
                if (request?.ServicePoint.BindIPEndPointDelegate != null) {
                    ServicePoint servicePoint = request.ServicePoint;
                    servicePoint.BindIPEndPointDelegate = (BindIPEndPoint) Delegate.Remove (servicePoint.BindIPEndPointDelegate, new BindIPEndPoint (bindEndPoint));
                }
            } catch { }
        }

        public event pWebRequest.RequestUpdateHandler DownloadProgress;

        public event pWebRequest.RequestCompleteHandler Finished;

        public event pWebRequest.RequestStartedHandler Started;

        public event pWebRequest.RequestUpdateHandler UploadProgress;

        public delegate void RequestCompleteHandler (pWebRequest request, Exception exception_0);

        public delegate void RequestStartedHandler (pWebRequest request);

        public delegate void RequestUpdateHandler (pWebRequest request, long current, long total);
    }
}