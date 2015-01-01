using System;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        
        StatusCode code;
        string NL = "\r\n";

        public Response(StatusCode code, string contentType, string content, string redirectoinPath)
        {
            this.code = code;

            //Add header lines (Content-Type, Content-Length, Date, [Location]):
            responseString  = GetStatusLine(code);
            responseString += "Content-Type: "   + contentType               + NL
                           +  "Content-Length: " + content.Length.ToString() + NL
                           +  "Date: "           + DateTime.Now.ToString()   + NL;
            if(redirectoinPath != String.Empty)
                responseString += "Location: " + redirectoinPath + NL;

            //Add empty new line:
            responseString += NL;

            //Add the actual content:
            responseString += content;
        }

        private string GetStatusLine(StatusCode code)
        {
            return string.Format("{0} {1} {2}\r\n", Configuration.ServerHTTPVersion, ((int)code).ToString(), code.ToString());
        }
    }
}
