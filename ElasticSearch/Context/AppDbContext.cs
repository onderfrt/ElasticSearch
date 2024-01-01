using Microsoft.EntityFrameworkCore;

namespace ElasticSearch.Context
{
    public sealed class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=LAPTOP-7I9ITMSM\\SQLEXPRESS;Initial Catalog=ElasticSearchTest;Integrated Security=True; TrustServerCertificate=True");
        }


        //public DbSet<Person> Persons { get; set; }
        public DbSet<Travel> Travels { get; set; }

    }

    public sealed class Travel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public sealed class Person
    {
        public int Id { get; set; }

        public int tckn_vkn { get; set; }

        public string ad_soyad_unvan { get; set; }

        public string uyrugu { get; set; }
    }
}
