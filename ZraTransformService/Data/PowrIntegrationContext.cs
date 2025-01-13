using Microsoft.EntityFrameworkCore;
using PowrIntegration.Data.Entities;

namespace PowrIntegration.Data
{
    public class PowrIntegrationDbContext(DbContextOptions<PowrIntegrationDbContext> options) : DbContext(options)
    {
        public DbSet<OutboxItem> OutboxItems { get; set; }
        public DbSet<PluItem> PluItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ZraClassificationClass> ZraClassificationClasses { get; set; }
        public DbSet<ZraClassificationCode> ZraClassificationCodes { get; set; }
        public DbSet<ZraClassificationFamily> ZraClassificationFamilies { get; set; }
        public DbSet<ZraClassificationSegment> ZraClassificationSegments { get; set; }
        public DbSet<ZraStandardCode> ZraStandardCodes { get; set; }
        public DbSet<ZraStandardCodeClass> ZraStandardCodeClasses { get; set; }
        public DbSet<ZraImportItem> ZraImportItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboxItem>()
                .HasKey(x => x.Id);                

            // OutboxItem configuration
            modelBuilder.Entity<OutboxItem>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd(); // Configure auto-increment

            modelBuilder.Entity<OutboxItem>()
                .Property(o => o.MessageType)
                .IsRequired();

            modelBuilder.Entity<OutboxItem>()
                .Property(o => o.MessageBody)
                .IsRequired();

            modelBuilder.Entity<PluItem>()
                .HasKey(p => p.PluNumber);

            modelBuilder.Entity<PluItem>()
                .Property(p => p.PluNumber)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<PluItem>()
                .HasOne(x => x.Recipe)
                .WithOne(x => x.Plu)
                .HasForeignKey((PluItem x) => x.PluNumber);

            modelBuilder.Entity<PluItem>()
                .HasMany(x => x.IngredientIn)
                .WithOne(x => x.IngredientPlu)
                .HasForeignKey(x => x.IngredientNumber);

            modelBuilder.Entity<Recipe>()
                .HasKey(x => x.PluNumber);

            modelBuilder.Entity<Recipe>()
                .HasMany(x => x.Ingredients)
                .WithOne(x => x.Recipe)
                .HasForeignKey(x => x.PluNumber);

            modelBuilder.Entity<Recipe>()
                .HasOne(x => x.Plu)
                .WithOne(x => x.Recipe)
                .HasForeignKey((Recipe x) => x.PluNumber);

            modelBuilder.Entity<Ingredient>()
                .HasKey(x => new { x.PluNumber, x.IngredientNumber });

            modelBuilder.Entity<Ingredient>()
                .HasOne(x => x.IngredientPlu)
                .WithMany(x => x.IngredientIn)
                .HasForeignKey(x => x.IngredientNumber);

            modelBuilder.Entity<Ingredient>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.PluNumber);

            modelBuilder.Entity<ZraClassificationSegment>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraClassificationSegment>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraClassificationSegment>()
                .HasMany(s => s.Families)
                .WithOne(s => s.Segment)
                .HasForeignKey(s => s.SegmentCode);

            modelBuilder.Entity<ZraClassificationClass>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraClassificationClass>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraClassificationClass>()
                .HasOne(c => c.Family)
                .WithMany(f => f.Classes)
                .HasForeignKey(c => c.FamilyCode);

            modelBuilder.Entity<ZraClassificationClass>()
                .HasMany(c => c.Codes)
                .WithOne(c => c.Class)
                .HasForeignKey(c => c.ClassCode);

            modelBuilder.Entity<ZraClassificationFamily>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraClassificationFamily>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraClassificationFamily>()
                .HasOne(f => f.Segment)
                .WithMany(s => s.Families)
                .HasForeignKey(f => f.SegmentCode);

            modelBuilder.Entity<ZraClassificationFamily>()
                .HasMany(f => f.Classes)
                .WithOne(c => c.Family)
                .HasForeignKey(c => c.FamilyCode);

            modelBuilder.Entity<ZraClassificationCode>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraClassificationCode>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraStandardCode>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraStandardCode>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraStandardCode>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Codes)
                .HasForeignKey(s => s.ClassCode);

            modelBuilder.Entity<ZraStandardCodeClass>()
                .HasKey(c => c.Code);

            modelBuilder.Entity<ZraStandardCodeClass>()
                .Property(p => p.Code)
                .IsRequired()
                .ValueGeneratedNever();

            modelBuilder.Entity<ZraStandardCodeClass>()
                .HasMany(c => c.Codes)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassCode);

            modelBuilder.Entity<ZraImportItem>()
                .HasKey(x => new { x.DeclarationNumber, x.ItemSequenceNumber });

            base.OnModelCreating(modelBuilder);
        }
    }
}
