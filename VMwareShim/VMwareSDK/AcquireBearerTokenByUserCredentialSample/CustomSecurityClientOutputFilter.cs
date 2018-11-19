using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;

namespace AcquireBearerTokenByUserCredentialSample
{
    class CustomSecurityClientOutputFilter : SendSecurityFilter
    {
        UsernameToken userToken = null;

        public CustomSecurityClientOutputFilter(CustomSecurityAssertion parentAssertion)
            : base(parentAssertion.ServiceActor, true)
        {
            userToken = new UsernameToken(parentAssertion.username, parentAssertion.password, PasswordOption.SendPlainText);
        }

        /// <summary>
        ///  SecureMessage 
        /// </summary>
        /// <param name="envelope">SoapEnvelope</param>
        /// <param name="security">Security</param>
        public override void SecureMessage(SoapEnvelope envelope, Security security)
        {
            security.Tokens.Add(userToken);
        }
    }
}
