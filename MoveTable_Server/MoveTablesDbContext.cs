using Microsoft.EntityFrameworkCore;
using MoveTable_Server.Models.User;

namespace MoveTable_Server
{
    public class MoveTablesDbContext : DbContext
    {
        // 建構函式
        public MoveTablesDbContext(DbContextOptions<MoveTablesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        // Model 屬性設定
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User 實體的配置
            modelBuilder.Entity<User>(entity =>
            {
                // 設定主鍵
                // ValueGeneratedOnAdd 插入新的實體時，值自動生成
                // UseIdentityColumn(10000, 1) 識別碼起始值和增量
                entity.Property(e => e.UserId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(10000, 1);

                // 設定外鍵關係
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Roles");
            });

            // Role 實體的配置
            modelBuilder.Entity<Role>(entity =>
            {
                // 設定主鍵
                // ValueGeneratedOnAdd 插入新的實體時，值自動生成
                // UseIdentityColumn(10000, 1) 識別碼起始值和增量
                entity.Property(e => e.RoleId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(10000, 1);
            });
        }


        // OnConfiguring
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 建立 IConfiguration 物件，讀取應用程式的設定資訊。
                IConfiguration Config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

                // 獲取 資料庫連線字串
                optionsBuilder.UseSqlServer(Config.GetConnectionString("MoveTables"));
            }
        }
    }
}