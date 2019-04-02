using System.Data.Entity;
using WinFormsSignalRAppl.Server.Entities;

namespace WinFormsSignalRAppl.Server.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(string conectionString) : base(conectionString)
        { }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new UserDbInitializer());
        }

        public DbSet<ClientFile> ClientFiles { get; set; }
    }

    public class UserDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    { }

}
