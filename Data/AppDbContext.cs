using Microsoft.EntityFrameworkCore;
using TrickcalServer.Models;

namespace TrickcalServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayerAccount> Players => Set<PlayerAccount>();
    public DbSet<PlayerCharacter> PlayerCharacters => Set<PlayerCharacter>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<GachaBanner> GachaBanners => Set<GachaBanner>();
    public DbSet<UserCard> UserCards => Set<UserCard>();
    public DbSet<UserCurrency> UserCurrencies => Set<UserCurrency>();
    public DbSet<UserPity> UserPities => Set<UserPity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── PlayerAccount ──
        modelBuilder.Entity<PlayerAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nickname).IsUnique();
            entity.Property(e => e.Nickname).HasMaxLength(20);
        });

        // ── PlayerCharacter ──
        modelBuilder.Entity<PlayerCharacter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlayerId, e.CharacterId }).IsUnique();
            entity.HasOne(e => e.Player)
                  .WithMany(p => p.Characters)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Card (사도) ──
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Tribe).HasMaxLength(30);
            entity.Property(e => e.Class).HasMaxLength(30);
            entity.Property(e => e.Grade).HasConversion<string>().HasMaxLength(10);
        });

        // ── GachaBanner ──
        modelBuilder.Entity<GachaBanner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PickupRate).HasPrecision(10, 6);
            entity.HasOne(e => e.PickupCard)
                  .WithMany()
                  .HasForeignKey(e => e.PickupCardId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── UserCard ──
        modelBuilder.Entity<UserCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CardId }).IsUnique();
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.HasOne(e => e.Card)
                  .WithMany()
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── UserCurrency ──
        modelBuilder.Entity<UserCurrency>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(50);
        });

        // ── UserPity ──
        modelBuilder.Entity<UserPity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(50);
        });

        // ══════════════════════════════════════
        //  시드 데이터
        // ══════════════════════════════════════
        SeedCards(modelBuilder);
        SeedBanners(modelBuilder);
        SeedTestUser(modelBuilder);
    }

    private static void SeedCards(ModelBuilder mb)
    {
        mb.Entity<Card>().HasData(
            // ── 1성 (10명) ──
            new Card { Id = 1,  Name = "리나",     Grade = CardGrade.Star1, Tribe = "인간",   Class = "전사",   BaseAtk = 120, BaseDef = 80  },
            new Card { Id = 2,  Name = "코볼",     Grade = CardGrade.Star1, Tribe = "수인",   Class = "궁수",   BaseAtk = 110, BaseDef = 70  },
            new Card { Id = 3,  Name = "미란다",   Grade = CardGrade.Star1, Tribe = "엘프",   Class = "마법사", BaseAtk = 130, BaseDef = 60  },
            new Card { Id = 4,  Name = "톤즈",     Grade = CardGrade.Star1, Tribe = "기계",   Class = "탱커",   BaseAtk = 90,  BaseDef = 140 },
            new Card { Id = 5,  Name = "피넛",     Grade = CardGrade.Star1, Tribe = "수인",   Class = "힐러",   BaseAtk = 80,  BaseDef = 90  },
            new Card { Id = 6,  Name = "쿠로",     Grade = CardGrade.Star1, Tribe = "마족",   Class = "암살자", BaseAtk = 140, BaseDef = 50  },
            new Card { Id = 7,  Name = "도트",     Grade = CardGrade.Star1, Tribe = "인간",   Class = "궁수",   BaseAtk = 115, BaseDef = 75  },
            new Card { Id = 8,  Name = "모스",     Grade = CardGrade.Star1, Tribe = "정령",   Class = "마법사", BaseAtk = 125, BaseDef = 65  },
            new Card { Id = 9,  Name = "버키",     Grade = CardGrade.Star1, Tribe = "기계",   Class = "전사",   BaseAtk = 118, BaseDef = 85  },
            new Card { Id = 10, Name = "릴리",     Grade = CardGrade.Star1, Tribe = "엘프",   Class = "힐러",   BaseAtk = 85,  BaseDef = 95  },

            // ── 2성 (6명) ──
            new Card { Id = 11, Name = "세라핀",   Grade = CardGrade.Star2, Tribe = "엘프",   Class = "마법사", BaseAtk = 200, BaseDef = 120 },
            new Card { Id = 12, Name = "카이론",   Grade = CardGrade.Star2, Tribe = "수인",   Class = "전사",   BaseAtk = 210, BaseDef = 150 },
            new Card { Id = 13, Name = "노바",     Grade = CardGrade.Star2, Tribe = "기계",   Class = "궁수",   BaseAtk = 190, BaseDef = 110 },
            new Card { Id = 14, Name = "루시안",   Grade = CardGrade.Star2, Tribe = "인간",   Class = "탱커",   BaseAtk = 160, BaseDef = 220 },
            new Card { Id = 15, Name = "유키",     Grade = CardGrade.Star2, Tribe = "정령",   Class = "힐러",   BaseAtk = 150, BaseDef = 180 },
            new Card { Id = 16, Name = "아스카",   Grade = CardGrade.Star2, Tribe = "마족",   Class = "암살자", BaseAtk = 230, BaseDef = 100 },

            // ── 3성 (3명) ──
            new Card { Id = 17, Name = "아르테미스", Grade = CardGrade.Star3, Tribe = "엘프",   Class = "궁수",   BaseAtk = 350, BaseDef = 200 },
            new Card { Id = 18, Name = "발키리",     Grade = CardGrade.Star3, Tribe = "인간",   Class = "전사",   BaseAtk = 380, BaseDef = 250 },
            new Card { Id = 19, Name = "오르페우스", Grade = CardGrade.Star3, Tribe = "정령",   Class = "마법사", BaseAtk = 400, BaseDef = 180 },

            // ── 엘다인 (1명) ──
            new Card { Id = 20, Name = "엘다인",   Grade = CardGrade.Eldain, Tribe = "신족",   Class = "지배자", BaseAtk = 500, BaseDef = 350 }
        );
    }

    private static void SeedBanners(ModelBuilder mb)
    {
        mb.Entity<GachaBanner>().HasData(
            new GachaBanner
            {
                Id = 1,
                Name = "신규 사도 픽업",
                PickupCardId = 18,          // 발키리 (3성)
                IsPickupEldain = false,
                PickupRate = 0.008m,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                IsActive = true
            },
            new GachaBanner
            {
                Id = 2,
                Name = "엘다인 특별 픽업",
                PickupCardId = 20,          // 엘다인
                IsPickupEldain = true,
                PickupRate = 0.006m,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }

    private static void SeedTestUser(ModelBuilder mb)
    {
        mb.Entity<UserCurrency>().HasData(
            new UserCurrency { UserId = "testuser", Eleaf = 10000, Gold = 5000 }
        );

        mb.Entity<UserPity>().HasData(
            new UserPity { UserId = "testuser", Faith = 0 }
        );
    }
}
