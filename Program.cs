using Microsoft.EntityFrameworkCore;
using TrickcalServer.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Orleans Silo ──
builder.Host.UseOrleans(silo =>
{
    silo.UseLocalhostClustering();          // 개발용 단일 노드 클러스터링
    silo.AddMemoryGrainStorageAsDefault();  // 개발용 인메모리 Grain 상태 저장
});

// ── EF Core + PostgreSQL ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// ── Redis 분산 캐시 ──
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Trickcal:";
});

// ── ASP.NET Core MVC + Swagger ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Trickcal Server API",
        Version = "v1",
        Description = "트릭컬 모티브 게임 서버 API"
    });
});

var app = builder.Build();

// ── 개발 환경: DB 자동 마이그레이션 + Swagger UI ──
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Trickcal Server v1");
        options.RoutePrefix = string.Empty; // 루트(/) 에서 Swagger UI 접근
    });
}

app.MapControllers();

app.Run();
