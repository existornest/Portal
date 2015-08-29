using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class LoginModelContext : DbContext
    {
        public DbSet<LoginModel> LoginModels { get; set; }
    }
}