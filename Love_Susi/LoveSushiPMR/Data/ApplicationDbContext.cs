using Microsoft.EntityFrameworkCore;
using LoveSushiPMR.Models.Entities;

namespace LoveSushiPMR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> AppRoles { get; set; }
        public DbSet<BonusAccount> BonusAccounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionDish> PromotionDishes { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<DeliveryZone> DeliveryZones { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        private static DateTime EnsureUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Глобальный конвертер для всех DateTime свойств
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => EnsureUtc(v),
                v => EnsureUtc(v)
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }

            // Составной ключ для PromotionDish
            modelBuilder.Entity<PromotionDish>()
                .HasKey(pd => new { pd.PromotionId, pd.DishId });

            modelBuilder.Entity<PromotionDish>()
                .HasOne(pd => pd.Promotion)
                .WithMany(p => p.PromotionDishes)
                .HasForeignKey(pd => pd.PromotionId);

            modelBuilder.Entity<PromotionDish>()
                .HasOne(pd => pd.Dish)
                .WithMany(d => d.PromotionDishes)
                .HasForeignKey(pd => pd.DishId);

            // Настройка связей User
            modelBuilder.Entity<User>()
                .HasOne(u => u.BonusAccount)
                .WithOne(ba => ba.User)
                .HasForeignKey<BonusAccount>(ba => ba.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка Order
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Настройка PromoCode - уникальный код
            modelBuilder.Entity<PromoCode>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<Promotion>()
                .HasMany(p => p.PromoCodes)
                .WithOne(pc => pc.Promotion)
                .HasForeignKey(pc => pc.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка decimal для PostgreSQL
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.BonusUsed)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Dish>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.DiscountPercent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.MaxDiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.MinOrderAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DeliveryZone>()
                .Property(d => d.DeliveryPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BonusAccount>()
                .Property(ba => ba.Balance)
                .HasPrecision(18, 2);

            // Seed данных
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Роли
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Администратор" },
                new Role { Id = 2, Name = "Пользователь" }
            );

            // Зоны доставки
            modelBuilder.Entity<DeliveryZone>().HasData(
                new DeliveryZone { Id = 1, Name = "Центр города", DeliveryPrice = 0, MinDeliveryTimeMinutes = 30, MaxDeliveryTimeMinutes = 50 },
                new DeliveryZone { Id = 2, Name = "Спутник", DeliveryPrice = 150, MinDeliveryTimeMinutes = 40, MaxDeliveryTimeMinutes = 60 },
                new DeliveryZone { Id = 3, Name = "Западный", DeliveryPrice = 100, MinDeliveryTimeMinutes = 35, MaxDeliveryTimeMinutes = 55 },
                new DeliveryZone { Id = 4, Name = "Северный", DeliveryPrice = 100, MinDeliveryTimeMinutes = 35, MaxDeliveryTimeMinutes = 55 }
            );

            // Категории
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Суши и роллы", SortOrder = 1, IconUrl = "🍣" },
                new Category { Id = 2, Name = "Сеты", SortOrder = 2, IconUrl = "🍱" },
                new Category { Id = 3, Name = "Лапша WOK", SortOrder = 3, IconUrl = "🍜" },
                new Category { Id = 4, Name = "Пицца", SortOrder = 4, IconUrl = "🍕" },
                new Category { Id = 5, Name = "Салаты", SortOrder = 5, IconUrl = "🥗" },
                new Category { Id = 6, Name = "Супы", SortOrder = 6, IconUrl = "🥣" },
                new Category { Id = 7, Name = "Напитки", SortOrder = 7, IconUrl = "🥤" },
                new Category { Id = 8, Name = "Десерты", SortOrder = 8, IconUrl = "🍰" }
            );

            // Блюда
            modelBuilder.Entity<Dish>().HasData(
                new Dish { Id = 1, Name = "Филадельфия классик", Description = "Лосось, сыр филадельфия, огурец, нори", Price = 420, WeightGrams = 250, CategoryId = 1, IsPopular = true, ImageUrl = "/images/dishes/philadelphia.jpg" },
                new Dish { Id = 2, Name = "Калифорния", Description = "Краб-микс, огурец, авокадо, икра тобико, нори", Price = 380, WeightGrams = 220, CategoryId = 1, IsPopular = true, ImageUrl = "/images/dishes/california.jpg" },
                new Dish { Id = 3, Name = "Дракон", Description = "Угорь, огурец, соус унаги, кунжут", Price = 520, WeightGrams = 280, CategoryId = 1, ImageUrl = "/images/dishes/dragon.jpg" },
                new Dish { Id = 4, Name = "Сет 'Любовь'", Description = "Филадельфия классик, Калифорния, Спайси ролл, Сяке маки - 32 шт.", Price = 1450, WeightGrams = 1000, CategoryId = 2, IsPopular = true, ImageUrl = "/images/dishes/set-love.jpg" },
                new Dish { Id = 5, Name = "Сет 'Семейный'", Description = "12 видов роллов - 64 шт. Идеально для компании!", Price = 2800, WeightGrams = 2200, CategoryId = 2, ImageUrl = "/images/dishes/set-family.jpg" },
                new Dish { Id = 6, Name = "WOK с курицей", Description = "Лапша удон, курица, овощи, соус терияки", Price = 350, WeightGrams = 350, CategoryId = 3, IsPopular = true, ImageUrl = "/images/dishes/wok-chicken.jpg" },
                new Dish { Id = 7, Name = "WOK с креветками", Description = "Лапша рисовая, креветки, овощи, соус острый", Price = 450, WeightGrams = 350, CategoryId = 3, ImageUrl = "/images/dishes/wok-shrimp.jpg" },
                new Dish { Id = 8, Name = "Пицца 'Маргарита'", Description = "Томатный соус, моцарелла, базилик", Price = 380, WeightGrams = 550, CategoryId = 4, IsPopular = true, ImageUrl = "/images/dishes/pizza-margarita.jpg" },
                new Dish { Id = 9, Name = "Пицца 'Пепперони'", Description = "Томатный соус, моцарелла, пепперони", Price = 450, WeightGrams = 600, CategoryId = 4, ImageUrl = "/images/dishes/pizza-pepperoni.jpg" },
                new Dish { Id = 10, Name = "Цезарь с курицей", Description = "Курица, салат романо, сыр пармезан, соус цезарь, крутоны", Price = 320, WeightGrams = 250, CategoryId = 5, ImageUrl = "/images/dishes/salad-caesar.jpg" },
                new Dish { Id = 11, Name = "Мисо суп", Description = "Тофу, водоросли вакаме, лук зелёный", Price = 120, WeightGrams = 300, CategoryId = 6, ImageUrl = "/images/dishes/soup-miso.jpg" },
                new Dish { Id = 12, Name = "Coca-Cola 0.5л", Description = "Освежающий напиток", Price = 80, WeightGrams = 500, CategoryId = 7, ImageUrl = "/images/dishes/cola.jpg" },
                new Dish { Id = 13, Name = "Тирамису", Description = "Классический итальянский десерт", Price = 250, WeightGrams = 150, CategoryId = 8, IsNew = true, ImageUrl = "/images/dishes/tiramisu.jpg" }
            );

            // Акции
            modelBuilder.Entity<Promotion>().HasData(
                new Promotion { Id = 1, Name = "Счастливые часы", Description = "Скидка 20% с 12:00 до 15:00", DiscountPercent = 20, StartDate = DateTime.UtcNow.AddYears(-1), EndDate = DateTime.UtcNow.AddYears(1), ImageUrl = "/images/promos/happy-hours.jpg" },
                new Promotion { Id = 2, Name = "День рождения", Description = "Скидка 15% в день рождения", DiscountPercent = 15, StartDate = DateTime.UtcNow.AddYears(-1), EndDate = DateTime.UtcNow.AddYears(1), ImageUrl = "/images/promos/birthday.jpg" }
            );

            // Курьеры
            modelBuilder.Entity<Courier>().HasData(
                new Courier { Id = 1, FullName = "Иванов Иван", Phone = "+373-777-12-34", Status = CourierStatus.Available },
                new Courier { Id = 2, FullName = "Петров Пётр", Phone = "+373-777-56-78", Status = CourierStatus.Available }
            );
        }
    }
}
