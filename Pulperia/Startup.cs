using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Pulperia.Models;

[assembly: OwinStartupAttribute(typeof(Pulperia.Startup))]
namespace Pulperia
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesandUsers();
        }

        private void CreateRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin rool   
                var role = new IdentityRole
                {
                    Name = "Admin"
                };
                roleManager.Create(role);
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Vendedor"))
            {
                var role = new IdentityRole
                {
                    Name = "Vendedor"
                };
                roleManager.Create(role);
            }

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Asociado"))
            {
                var role = new IdentityRole
                {
                    Name = "Asociado"
                };
                roleManager.Create(role);
            }

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Pulpero"))
            {
                var role = new IdentityRole
                {
                    Name = "Pulpero"
                };
                roleManager.Create(role);
            }
        }
    }
}
