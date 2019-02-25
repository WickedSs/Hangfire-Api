using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;

namespace hangfire_api.Models {
    public class gitRepo_Context : DbContext
    {

        public gitRepo_Context(DbContextOptions<gitRepo_Context> options) : base(options) {

        }

        public DbSet<gitRepo> gitRepos { get; set; }

    }
}