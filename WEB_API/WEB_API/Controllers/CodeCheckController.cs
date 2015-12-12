using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WEB_API.Models;
using XEuropeApp;

namespace WEB_API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CodeCheckController : ApiController
    {
        private DtouchMarker _marker;
        private MarkerDetector _detector;
        private MarkerHelper _markerHelper;

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Welcome");

        }
        

        
        public HttpResponseMessage Post([FromBody] JsonImage baseImage)
        {
            if (baseImage == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "byte[] is null!");
            }
            _markerHelper = new MarkerHelper(null);
            var path =  System.Web.Hosting.HostingEnvironment.MapPath("/SampleImage/");
            var fileName = path;
            byte[] bytes;
            try
            {
                fileName += "temp_" + DateTime.Now.ToFileTimeUtc() + ".png";
                bytes = baseImage.ImageBytes;
                Converters.byteArrayToImage(bytes).Save(fileName, ImageFormat.Png);
                _marker = new DtouchMarker();
                _detector = new MarkerDetector(new HIPreference());
                //use 1 for size modifier in constructor, mean it will use the whole image for detection
                _markerHelper = new MarkerHelper(_detector, 1);
                _markerHelper.ProcessFrame(_marker, fileName);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
 
            }
            //delete the temp file
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            //sending response
            var resp = new JsonResponse {message = _marker.getCodeKey(), error = !_markerHelper.IsMarkerDetected};
           
            return Request.CreateResponse(HttpStatusCode.OK,resp);
        }




    }
}
