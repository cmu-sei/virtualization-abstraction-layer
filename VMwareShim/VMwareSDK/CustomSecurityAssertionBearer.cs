namespace AppUtil
{
    using System.Xml;
    using Microsoft.Web.Services3;
    using Microsoft.Web.Services3.Design;
    using Microsoft.Web.Services3.Security;
    using Microsoft.Web.Services3.Security.Tokens;

    /// <summary>
    /// Custom policy assertion that applies security to a SOAP message exchange.
    /// </summary>
    internal class CustomSecurityAssertionBearer : SecurityPolicyAssertion
    {
        public XmlElement BinaryToken { get; set; }

        public CustomSecurityAssertionBearer()
            : base()
        { }

        public override SoapFilter CreateClientOutputFilter(FilterCreationContext context)
        {
            return new CustomSecurityClientOutputFilterBearer(this);
        }

        public override SoapFilter CreateClientInputFilter(FilterCreationContext context)
        {
            return null;
        }

        public override SoapFilter CreateServiceInputFilter(FilterCreationContext context)
        {
            return null;
        }

        public override SoapFilter CreateServiceOutputFilter(FilterCreationContext context)
        {
            return null;
        }
    }

    /// <summary>
    /// Custom class for filtering outgoing SOAP messages that are secured 
    /// by a digital signature, encryption, or authentication.
    /// </summary>
    internal class CustomSecurityClientOutputFilterBearer : SendSecurityFilter
    {
        IssuedToken issuedToken = null;

        /// <summary>
        /// Creates CustomSecurityClientOutputFilterBearer object
        /// </summary>
        /// <param name="parentAssertion">Parent assertion</param>
        public CustomSecurityClientOutputFilterBearer(CustomSecurityAssertionBearer parentAssertion)
            : base(parentAssertion.ServiceActor, true)
        {
            issuedToken = new IssuedToken(parentAssertion.BinaryToken);
        }

        /// <summary>
        ///  Secures the SOAP message before its sent to the server 
        /// </summary>
        /// <param name="envelope">Soap envelope</param>
        /// <param name="security">Security header element</param>
        public override void SecureMessage(SoapEnvelope envelope, Security security)
        {
            security.Tokens.Add(issuedToken);
        }
    }
}
