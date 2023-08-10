using Microsoft.EntityFrameworkCore;

namespace IdentityByCertificate.Models
{
 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<CertificateModel> Certificates { get; set; }
    }
}
