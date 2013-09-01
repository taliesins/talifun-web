using System.IO;
using System.Reflection;
using System.Web;

namespace Talifun.Web.StaticFile
{
    public sealed class StaticFileManager
    {
        private const int BufferSize = 32768;
        private const long MaxFileSizeToServe = int.MaxValue;
        private const uint ErrorTheRemoteHostClosedTheConnection = 0x80072746; //WSAECONNRESET (10054)

        private readonly ICacheManager _cacheManager;
        private readonly IRetryableFileOpener _retryableFileOpener;
        private readonly IMimeTyper _mimeTyper;
        private readonly IHasher _hasher;
        private readonly IHttpRequestHeaderHelper _httpRequestHeaderHelper;
        private readonly MimeSettingProvider _mimeSettingProvider;
        private IHttpResponseHeaderHelper _httpResponseHeaderHelper;
        private IHttpRequestResponder _httpRequestResponder;
        private WebServerType _webServerType;

        private StaticFileManager()
        {
            _cacheManager = new HttpCacheManager();
            _retryableFileOpener = new RetryableFileOpener();
            _mimeTyper = new MimeTyper(_cacheManager);
            _hasher = new Md5Hasher(_retryableFileOpener);
            _httpRequestHeaderHelper = new HttpRequestHeaderHelper();
            _mimeSettingProvider = new MimeSettingProvider();
        }

        public static StaticFileManager Instance
        {
            get
            {
                return Nested.Instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly StaticFileManager Instance = new StaticFileManager();
        }

        /// <summary>
        /// Initialize the classes that can only be determined once we have a requested passed to us by the web server.
        /// </summary>
        /// <param name="context"></param>
        private void Initialize(HttpContextBase context)
        {   
            if (_webServerType == WebServerType.NotSet)
            {
                _webServerType = WebServerDetector.DetectWebServerType(context);
            }

            if (_httpResponseHeaderHelper == null)
            {
                _httpResponseHeaderHelper = new HttpResponseHeaderHelper(_webServerType);
            }

            if (_httpRequestResponder == null)
            {
                _httpRequestResponder = new HttpRequestResponder(_httpRequestHeaderHelper, _httpResponseHeaderHelper);
            }
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var physicalFilePath = context.Request.PhysicalPath;
            var file = new FileInfo(physicalFilePath);

            ProcessRequest(context, file);
        }

        public void ProcessRequest(HttpContextBase context, FileInfo file)
        {
            var fileEntity = new FileEntityResponder(_cacheManager, _retryableFileOpener, _mimeTyper, _hasher, MaxFileSizeToServe, BufferSize, _mimeSettingProvider, file);
            ProcessRequest(context, fileEntity);
        }

        public void ProcessRequest(HttpContextBase context, Assembly assembly, string resourcePath)
        {
            var fileEntity = new EmbeddedResourceEntityResponder(_cacheManager, _mimeTyper, _hasher, MaxFileSizeToServe, BufferSize, _mimeSettingProvider, assembly, resourcePath);
            ProcessRequest(context, fileEntity);
        }

        public void ProcessRequest(HttpContextBase context, IEntityResponder entityResponder)
        {
            var request = context.Request;
            var response = context.Response;

            if (_webServerType == WebServerType.NotSet || _httpResponseHeaderHelper == null || _httpRequestResponder == null)
            {
                Initialize(context);
            }

            //We don't want to use up all the servers memory keeping a copy of the file, we just want to stream file to client
            response.BufferOutput = false;

            try
            {
                _httpRequestResponder.ServeRequest(request, response, entityResponder);
            }
            catch (HttpException httpException)
            {
                //Client disconnected half way through us sending data
                if (httpException.ErrorCode != ErrorTheRemoteHostClosedTheConnection)
                    return;

                throw;
            }
        }
    }
}