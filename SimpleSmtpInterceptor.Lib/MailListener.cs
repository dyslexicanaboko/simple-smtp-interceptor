using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleSmtpInterceptor.Lib
{
    /* All credit for the basis of this code goes to the author "Al Forno" of this comment on Code-Project
     * https://www.codeproject.com/Tips/286952/create-a-simple-smtp-server-in-csharp?msg=4363652#xx4363652xx
     *
     * I have modified this code for my needs and cleaned it up to my liking. */
    public class MailListener 
        : TcpListener
    {
        private bool _keepListening = true;

        private readonly bool _verboseOutput;

        public MailListener(IPAddress localaddr, int port, bool verboseOutput)
            : base(localaddr, port)
        {
            _verboseOutput = verboseOutput;
        }

        //Picked up some knowledge about how to multi thread the requests from this answer
        //https://stackoverflow.com/questions/5339782/how-do-i-get-tcplistener-to-accept-multiple-connections-and-work-with-each-one-i
        public void StartListening()
        {
            Start();

            while (_keepListening)
            {
                var client = AcceptTcpClient();

                client.ReceiveTimeout = 5000;

                Task.Factory.StartNew((obj) => 
                {
                    var c = (TcpClient)obj;

                    using (var handler = new ClientHandler(c, _verboseOutput))
                    {
                        handler.HandleRequest();
                    }
                }, client);
            }
        }

        public void StopListening()
        {
            _keepListening = false;

            Stop();
        }
    }
}
