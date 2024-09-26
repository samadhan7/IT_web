using Microsoft.EntityFrameworkCore;
namespace GTL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
            this.Database.SetCommandTimeout(300);
        }



    }
}
