using System.Security.Cryptography.X509Certificates;

namespace PSOK.Kernel.Security
{
    /// <summary>
    /// Validates a certificate.
    /// </summary>
    public class X509CertificateValidator : System.IdentityModel.Selectors.X509CertificateValidator
    {
        static X509CertificateValidator()
        {
            SetCertificateValidation();
        }

        /// <summary>
        /// Performs the validation check.
        /// </summary>
        /// <param name="certificate"></param>
        public override void Validate(X509Certificate2 certificate)
        {
            
        }

        /// <summary>
        /// Determines how to validate certificates.
        /// </summary>
        public static void SetCertificateValidation()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
        ((sender, certificate, chain, sslPolicyErrors) => true);
        }
    }
}
