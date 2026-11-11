using Black_order.Models.Guides;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Data;

/// <summary>
/// Development-only sample content so the guides module can be verified end to end.
/// Replace the video URLs with real content before going to production.
/// </summary>
public static class GuidesSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await dbContext.Guides.AnyAsync())
            {
                return;
            }

            var leveling = new GuideCategory
            {
                Name = "Leveling",
                Slug = "leveling",
                Description = "Rutas y consejos para subir de nivel rápido.",
                SortOrder = 1
            };

            var mounts = new GuideCategory
            {
                Name = "Mounts & Transmog",
                Slug = "mounts-transmog",
                Description = "Cómo farmear monturas y transfiguraciones.",
                SortOrder = 2
            };

            var gold = new GuideCategory
            {
                Name = "Gold Making",
                Slug = "gold-making",
                Description = "Profesiones, subastas y estrategias de oro.",
                SortOrder = 3
            };

            var now = DateTimeOffset.UtcNow;

            var guides = new List<Guide>
            {
                new()
                {
                    Title = "Cómo subir del 1 al 80 en menos de 12 horas",
                    Slug = "subir-nivel-1-80-rapido",
                    Summary = "Ruta de misiones y dungeons optimizada para nuevo personaje.",
                    ContentHtml = """
                        <p>Esta guía resume la ruta de leveling más eficiente para un personaje nuevo.</p>
                        <h2>1. Preparación</h2>
                        <ul>
                          <li>Compra consumibles de experiencia si están disponibles.</li>
                          <li>Activa la montura en cuanto el nivel lo permita.</li>
                        </ul>
                        <h2>2. Ruta por zonas</h2>
                        <p>Encadena misiones principales y cierra cada zona con su dungeon asociado.</p>
                        """,
                    Level = "Beginner",
                    Expansion = "The War Within",
                    IsPublished = true,
                    PublishedAt = now.AddDays(-7),
                    CreatedAt = now.AddDays(-8),
                    UpdatedAt = now.AddDays(-7),
                    ViewCount = 1240,
                    Category = leveling,
                    Media =
                    {
                        new GuideMedia
                        {
                            Provider = "youtube",
                            Url = "https://www.youtube.com/results?search_query=world+of+warcraft+leveling+guide",
                            Caption = "Ruta de leveling en video (reemplazar por el video definitivo).",
                            DurationSeconds = 900,
                            SortOrder = 0
                        }
                    }
                },
                new()
                {
                    Title = "Ruta de monturas raras: por dónde empezar",
                    Slug = "ruta-monturas-raras",
                    Summary = "Orden recomendado, requisitos y addons para farmear monturas.",
                    ContentHtml = """
                        <p>Farming de monturas es cuestión de rutas y paciencia. Empieza por las que tienen mayor probabilidad.</p>
                        <h2>Addons recomendados</h2>
                        <ul>
                          <li>Rarity para llevar la cuenta de intentos.</li>
                          <li>HandyNotes para localizar rares.</li>
                        </ul>
                        <h2>Orden sugerido</h2>
                        <p>Rares de zona, luego mazmorras y por último raids antiguas.</p>
                        """,
                    Level = "Intermediate",
                    Expansion = "Dragonflight",
                    IsPublished = true,
                    PublishedAt = now.AddDays(-3),
                    CreatedAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-3),
                    ViewCount = 860,
                    Category = mounts,
                    Media =
                    {
                        new GuideMedia
                        {
                            Provider = "youtube",
                            Url = "https://www.youtube.com/results?search_query=wow+mount+farming+guide",
                            Caption = "Farming de monturas en video (reemplazar por el video definitivo).",
                            DurationSeconds = 1200,
                            SortOrder = 0
                        }
                    }
                },
                new()
                {
                    Title = "Cómo empezar a hacer oro con profesiones",
                    Slug = "oro-con-profesiones",
                    Summary = "Flujo de trabajo semanal para generar oro sin depender de raids.",
                    ContentHtml = """
                        <p>Las profesiones son la fuente de oro más estable para un jugador casual.</p>
                        <h2>Rutina semanal</h2>
                        <ol>
                          <li>Revisa precios de materiales al inicio de la semana.</li>
                          <li>Craftea solo lo que tenga margen positivo.</li>
                          <li>Publica en subasta en tandas pequeñas.</li>
                        </ol>
                        """,
                    Level = "Beginner",
                    Expansion = "The War Within",
                    IsPublished = true,
                    PublishedAt = now.AddDays(-1),
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-1),
                    ViewCount = 310,
                    Category = gold,
                    Media =
                    {
                        new GuideMedia
                        {
                            Provider = "youtube",
                            Url = "https://www.youtube.com/results?search_query=wow+gold+making+professions",
                            Caption = "Estrategias de oro en video (reemplazar por el video definitivo).",
                            DurationSeconds = 780,
                            SortOrder = 0
                        }
                    }
                }
            };

            await dbContext.Guides.AddRangeAsync(guides);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} sample guides.", guides.Count);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Guides seed skipped. Run 'dotnet ef database update' to create the schema first.");
        }
    }
}
