using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            this.LoadRedirectionRules(redirectionMatrixPath);

            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, portNumber);
            this.serverSocket.Bind(endpoint);
        }

        public void StartServer()
        {
            serverSocket.Listen(500); //Listen to connections, with large backlog.
            while(true)
            {
                //Accept connections and start thread for each accepted connection.
                Socket client = serverSocket.Accept();
                ThreadPool.QueueUserWorkItem(HandleConnection, client);
            }
        }

        public void HandleConnection(object obj)
        {
            Socket clientSocket = (Socket)obj;
            byte[] data = new byte[1024];
            clientSocket.ReceiveTimeout = 0; //indicates an infinite time-out period.
            
            while(true) //Receive requests until remote client closes the socket.
            {
                try
                {
                    //Getting request:
                    int length = clientSocket.Receive(data);
                    if(length == 0){ //Request length == 0. Done.
                        Console.WriteLine("Client closed.");
                        break;
                    }
                    String requestString = Encoding.ASCII.GetString(data, 0, length);
                    Console.WriteLine(requestString);                                                           //TESTING.

                    //Create a Request object using received request string:
                    Request request = new Request(requestString);

                    //Handle the request and get response:
                    Response response = HandleRequest(request);

                    //Send response back to client:
                    Console.WriteLine("{0}\r\n\r\n{1}\r\n", response.ResponseString, "-------------------");    //TESTING.
                    clientSocket.Send( Encoding.ASCII.GetBytes(response.ResponseString) );
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }

            clientSocket.Close();
        }

        Response HandleRequest(Request request)
        {
            string content = String.Empty;
            string redirectionPath = String.Empty;
            string relative;
            string path;
            StatusCode status = StatusCode.OK;
            Response response;

            try
            {
                bool parsing = request.ParseRequest();
                string internalURI = request.relativeURI.Substring(1,request.relativeURI.Length-1); //Delete starting '/'.
                string redirectRelative = GetRedirectionPagePathIfExists(internalURI);

                if(!parsing){ //Check for bad request.
                    status = StatusCode.BadRequest;
                    relative = Configuration.BadRequestDefaultPageName;
                }
                else if(redirectRelative != String.Empty){ //Check for redirect.
                    string host = request.HeaderLines["Host"].StartsWith("http://")
                        ? request.HeaderLines["Host"]
                        : "http://" + request.HeaderLines["Host"];
                    redirectionPath = host + '/' + redirectRelative;
                    status = StatusCode.Redirect;
                    relative = Configuration.RedirectionDefaultPageName;
                }
                else
                    relative = internalURI;

                //Map the relativeURI in request to get the physical path of the resource:
                relative = relative==String.Empty ? Configuration.DefaultPageName : relative;
                path = Configuration.RootPath + '\\' + relative;

                //Check file exists then read it:
                if(!File.Exists(@path)){
                    status = StatusCode.NotFound;
                    relative = Configuration.NotFoundDefaultPageName;
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
                
                //In case of exception, return internal server error:
                status = StatusCode.InternalServerError;
                relative = Configuration.InternalErrorDefaultPageName;
            }

            //Read file:
            path = Configuration.RootPath + '\\' + relative;
            content = System.IO.File.ReadAllText(@path);

            //Create response:
            response = new Response(status, "text/html", content, redirectionPath);

            return response;
        }


        /// <summary>
        /// Gets the redirection page path IF exist.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The redirection path, empty string otherwise</returns>
        private string GetRedirectionPagePathIfExists(string relativePath)
        {
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
            {
                return Configuration.RedirectionRules[relativePath];
            }

            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            if (!File.Exists(filePath))
            {
                Logger.LogException(new Exception("Default Page " + defaultPageName + " doesn't exist"));
                return string.Empty;
            }
            StreamReader reader = new StreamReader(filePath);
            string file = reader.ReadToEnd();
            reader.Close();
            return file;

        }

        /// <summary>
        /// Loads the redirection rules from the configuration file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                StreamReader reader = new StreamReader(filePath);


                Configuration.RedirectionRules = new Dictionary<string, string>();

                while (!reader.EndOfStream)
                {
                    string temp = reader.ReadLine();
                    string[] result = temp.Split(',');
                    Configuration.RedirectionRules.Add(result[0], result[1]);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                Environment.Exit(1);
            }
        }


    }
}
