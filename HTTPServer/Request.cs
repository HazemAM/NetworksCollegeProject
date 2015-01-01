using System;
using System.Collections.Generic;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLines;
        RequestMethod method;     //Not used 'cause only GET requests are supported in the mean time.
        public string relativeURI;
        private Dictionary<string, string> headerLines;
        private int lastHeaderLine;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion; //Not used 'cause only HTML/1.1 is supported in the mean time.
        string requestString;
        string[] contentLines;   //Not used 'cause only GET requests are supported in the mean time (hence, no content).

        public Request(string requestString)
        {
            this.requestString = requestString;
        }

        /// <summary>Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error</summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            //Parse the receivedRequest using the \r\n delimeter:
            requestLines = requestString.Split(new String[]{"\r\n"}, StringSplitOptions.None);

            //Check that there is atleast 3 lines; request line, a host header line, and a blank line:
            if(requestLines.Length < 3)
                return false;

            //Parse request line:
            if( !ParseRequestLine() )
                return false;

            //Load header lines into dictionary:
            if( !LoadHeaderLines() )
                return false;

            //Validate blank line exists:
            if( !ValidateBlankLine() )
                return false;

            //If no Host header (HTTP server ver is 1.1), there is a problem:
            if(!headerLines.ContainsKey("Host")) //httpVersion==HTTPVersion.HTTP11 &&
                return false;

            return true;
        }

        private bool ParseRequestLine()
        {
            try{
                string[] requestLine = requestLines[0].Split(' ');
                if(requestLine[0] != "GET")
                    return false;
                else method = RequestMethod.GET;

                relativeURI = requestLine[1];
                if( !IsURI(relativeURI) )
                    return false;

                if( !SetHTTPVer(requestLine[2]) )
                    return false;
            }
            catch { return false; }

            return true;
        }

        private bool SetHTTPVer(string httpString)
        {
            string[] http = httpString.Split('/');
            
            if(http[0] != "HTTP")
                return false;
            else if(http.Length == 1){
                httpVersion = HTTPVersion.HTTP09;
                return false;
            }
            else{
                switch(http[1])
                {
                    case "1.0": httpVersion=HTTPVersion.HTTP10; return false;
                    case "1.1": httpVersion=HTTPVersion.HTTP11; break;
                    default: return false;
                }
            }

            return true;
        }

        private bool IsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {
            headerLines = new Dictionary<string,string>();
            try{ //For any line that has a problem with indices [0] and [1] (which must have).
                string[] line;
                for(int i=1; i<requestLines.Length; i++){
                    if(requestLines[i] == String.Empty){
                        lastHeaderLine = i;
                        break;
                    }
                    line = requestLines[i].Split(new String[]{": "}, StringSplitOptions.None); //No ":".
                    headerLines.Add(line[0], line[1]);
                }
            }
            catch { return false; }

            return true;
        }

        private bool ValidateBlankLine() //For GET only requests, the blank line must be the last line (always no content).
        {
            if(requestLines[lastHeaderLine] == String.Empty)
                return true;
            else
                return false;
        }

    }
}
