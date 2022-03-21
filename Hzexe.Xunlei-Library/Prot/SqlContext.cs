using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzexe.Xunlei_Library.Prot
{
    internal class SqlContext : DbContext
    {
        public DbSet<TaskBase> TaskBases { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SqlContext()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SqlContext(DbContextOptions<SqlContext> options)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
          : base(options)
        {
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // if (!optionsBuilder.IsConfigured) {
           //     optionsBuilder.UseSqlite(@"DataSource=D:\Program Files\Thunder\profiles\TaskDb.dat;");
           // }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaskBase>().HasNoKey().ToTable("TaskBase");
            modelBuilder.Entity<TaskBase>().Property(x => x.Name).HasColumnType("nvarchar(2000)");
            modelBuilder.Entity<TaskBase>().Property(x => x.Url).HasColumnType("nvarchar(2000)");
            
        }

    }

   
}
