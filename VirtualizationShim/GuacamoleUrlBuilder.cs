using Step.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VirtualizationShim
{
    public class GuacamoleUrlBuilder
    {
        private string guacHost;
        private int guacPort;
        private Crypto crypto = new Crypto( "REDACTED_REDACTED_REDACTED_REDACTED_REDACTED" );

        public GuacamoleUrlBuilder( string guacHost, int guacPort )
        {
            this.guacHost = guacHost;
            this.guacPort = guacPort;
        }

        public string buildUrl( string host, int port, string username, string password )
        {
            string url = "http://" + guacHost + ":" + guacPort + "/#/client/";

            url += encodeRequestedConnectionToken( "" );

            url += "?step-token=";

            url += encodeStepToken( host, port, username, password );

            return url;
        }

        public string encodeStepToken( string host, int port, string username, string password )
        {
            string connectionDetail = String.Format( "id={0}&hostname={1}&username={2}&password={3}", "This_Is_A_Test_Exercise", host, username, password );
            string connectionDetailToken = HttpServerUtility.UrlTokenEncode( Encoding.ASCII.GetBytes( crypto.Encrypt( connectionDetail ) ) );
            return connectionDetailToken;
        }

        public string encodeRequestedConnectionToken( string requestedConnection )
        {
            string plaintext = requestedConnection + "^c^guacamole-auth-step";
            byte [] plainBytes = Encoding.Default.GetBytes( plaintext );
            for( int i = 0; i < plainBytes.Length; i++ )
                if( plainBytes [i] == '^' )
                    plainBytes [i] = 0;

            return Convert.ToBase64String( plainBytes );
        }
    }
}
