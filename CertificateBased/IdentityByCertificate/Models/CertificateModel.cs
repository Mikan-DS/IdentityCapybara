using System.ComponentModel.DataAnnotations;

namespace IdentityByCertificate.Models
{
    /// <summary>
    /// Модель для хранения информации о сертификате.
    /// </summary>
    public class CertificateModel
    {
        /// <summary>
        /// Отпечаток сертификата.
        /// </summary>
        [Key]
        public string Thumbprint { get; set; }

        /// <summary>
        /// Наименование клиента, связанного с сертификатом.
        /// </summary>
        [Required]
        public string Client { get; set; }

        /// <summary>
        /// Определяет, активен ли сертификат.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
