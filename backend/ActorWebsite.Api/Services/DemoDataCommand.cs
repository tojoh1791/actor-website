using ActorWebsite.Api.Contracts;
using ActorWebsite.Api.Data;
using ActorWebsite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ActorWebsite.Api.Services;

// Explicit local command: ordinary startup never adds demo content.
public static class DemoDataCommand
{
    public static async Task<int> RunAsync(IServiceProvider services)
    {
        if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            Console.Error.WriteLine("Demodata kan bare legges inn i Development-miljøet.");
            return 1;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            Console.Error.WriteLine("Kjør npm run db:migrate før du legger inn demodata.");
            return 1;
        }

        var now = services.GetRequiredService<TimeProvider>().GetUtcNow();
        var oslo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, oslo).DateTime);
        var events = CreateEvents(today, now, oslo);
        var bookings = CreateBookings(today, now);

        // Stable IDs make reruns additive: preserve edits, status changes and existing content.
        var eventIds = events.Select(item => item.Id).ToArray();
        var existingEvents = await db.Events.Where(item => eventIds.Contains(item.Id))
            .Select(item => item.Id).ToListAsync();
        var bookingIds = bookings.Select(item => item.Id).ToArray();
        var existingBookings = await db.BookingRequests.Where(item => bookingIds.Contains(item.Id))
            .Select(item => item.Id).ToListAsync();
        var newEvents = events.Where(item => !existingEvents.Contains(item.Id)).ToArray();
        var newBookings = bookings.Where(item => !existingBookings.Contains(item.Id)).ToArray();
        db.Events.AddRange(newEvents);
        db.BookingRequests.AddRange(newBookings);
        await db.SaveChangesAsync();
        Console.WriteLine($"La til {newEvents.Length} demoarrangementer og {newBookings.Length} demoforespørsler. Eksisterende innhold ble beholdt.");
        return 0;
    }

    private static ActorEvent[] CreateEvents(DateOnly today, DateTimeOffset now, TimeZoneInfo oslo)
    {
        ActorEvent Event(int number, int days, string title, string location, string city,
            string description, bool published = true)
        {
            var start = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(
                today.AddDays(days).ToDateTime(new TimeOnly(18, 0)), oslo));
            return new ActorEvent
            {
                Id = Id(1, number),
                Title = title + " (demo)",
                Description = "Dette er et fiktivt demoarrangement.\n\n" + description,
                StartsAt = start,
                EndsAt = start.AddMinutes(90),
                Location = location,
                Address = city + ", Norge",
                Organizer = location + " – demoarrangør",
                IsPublished = published,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now
            };
        }

        return
        [
            Event(1, 7, "Uten frykt – med håp", "Fjordlys kulturhus", "Oslo",
                "En personlig kveld med Arben Bala om tilhørighet, viktige valg og muligheten til å begynne på nytt. Etter foredraget åpner vi for spørsmål fra publikum. Dørene åpner en halvtime før start. Passer for voksne og ungdom fra 16 år."),
            Event(2, 14, "Balkanbrødre: historien bak boken", "Elvebyen bibliotek", "Drammen",
                "Møt Arben Bala til en samtale om livet som inspirerte romanen Balkanbrødre. Vi snakker om identitet, familie og reisen mellom Norge og Kosovo. Publikum inviteres til å stille spørsmål etter samtalen."),
            Event(3, 21, "Veien videre", "Møteplassen kulturarena", "Lillestrøm",
                "Et møte om utenforskap, ansvar og nye muligheter. Arben deler erfaringer fra eget liv og inviterer til refleksjon og samtale."),
            Event(4, 28, "Fra livshistorie til scenen", "Brygga litteraturhus", "Fredrikstad",
                "Hvordan blir egne erfaringer til fortellinger som berører andre? Arben Bala forteller om skuespilleryrket og møtet med publikum. Kvelden kombinerer samtale, korte fortellinger og spørsmål fra salen.\n\nDet blir en kort pause underveis. Lokalet har trinnfri inngang. Dette lengre demoeksempelet kan brukes til å vise hvordan Mer-knappen utvider arrangementskortet."),
            Event(5, 42, "En kveld med Arben Bala", "Vestbyen kulturforum", "Bergen",
                "Bli med på en kveld med historier om livet, kunsten og det å høre til. Vi setter av god tid til spørsmål og en uformell samtale med publikum."),
            Event(6, -14, "Fortellinger om tilhørighet", "Fjordlys kulturhus", "Oslo",
                "Et tidligere demoarrangement. Bruk dette til å vise historikken i kontrollpanelet og at tidligere arrangementer ikke vises blant de kommende."),
            Event(7, 35, "Uten frykt – ny møteplass", "Solgløtt frivilligsentral", "Moss",
                "Utkast til en ny foredragskveld. Se over dato, sted og innhold før publisering. Dette arrangementet er foreløpig bare synlig for administrator.", published: false)
        ];
    }

    private static BookingRequest[] CreateBookings(DateOnly today, DateTimeOffset now)
    {
        BookingRequest Booking(int number, int? days, string name, string organization,
            string type, string city, int participants, string message, string status = "new")
        {
            // Use the same normalization and content hash as the public booking form.
            var input = new BookingInput(Id(2, number), name, $"demo{number}@example.test", null,
                organization + " (demo)", type, days is { } offset ? today.AddDays(offset) : null,
                city, participants, "DEMO – fiktiv forespørsel.\n\n" + message, null, null);
            var item = input.ToEntity(now.AddHours(-number * 3));
            item.Status = status;
            item.UpdatedAt = now;
            return item;
        }

        return
        [
            Booking(1, 45, "Emma Eksempel", "Nordlys bibliotek", "talk", "Oslo", 100,
                "Vi ønsker foredraget Uten frykt for et voksent publikum. Gjerne 60 minutter foredrag og 30 minutter spørsmål. Har du anledning, og hva vil et slikt oppdrag koste?"),
            Booking(2, 52, "Jonas Eksempel", "Leseglede litteraturfestival", "book", "Drammen", 80,
                "Vi ønsker en boksamtale om Balkanbrødre og historien bak romanen. Vi stiller med en samtaleleder og ønsker å avslutte med spørsmål fra publikum."),
            Booking(3, null, "Sara Eksempel", "Brobygger ungdomsforum", "other", "Lillestrøm", 45,
                "Vi planlegger en temakveld om tilhørighet og gode valg. Datoen er ikke bestemt. Kan vi ta en prat om et samarbeid tilpasset ungdom og unge voksne?"),
            Booking(4, 60, "Noah Eksempel", "Samspill videregående", "talk", "Fredrikstad", 180,
                "Vi har vært i kontakt om en temadag for elevene. Vi undersøker nå budsjett og ønsker å avklare varighet og tekniske behov før vi bekrefter.", "inProgress"),
            Booking(5, 70, "Maja Eksempel", "Sceneverkstedet", "acting", "Bergen", 12,
                "Vi ønsker å diskutere en rolle i en kortfilm. Opptak er planlagt over to dager. Dette er et privat skuespilleroppdrag og skal ikke publiseres som et arrangement.", "inProgress"),
            Booking(6, 56, "Oliver Eksempel", "Fellesrom kulturforening", "talk", "Moss", 120,
                "Vi har avtalt en foredragskveld og bekreftet dato og honorar i demoen. Arrangementet er åpent for publikum. Opprett gjerne et utkast med tid og praktisk informasjon.", "agreed"),
            Booking(7, 63, "Linnea Eksempel", "Historiefilm produksjon", "acting", "Oslo", 8,
                "Et avtalt skuespilleroppdrag i en fiktiv reklamefilm. Viser at Avtalt-status ikke gir mulighet til å opprette offentlig arrangement for denne oppdragstypen.", "agreed"),
            Booking(8, 30, "Elias Eksempel", "Perspektiv konferanse", "talk", "Trondheim", 200,
                "Vi forespurte et innlegg på konferansen, men datoen passet ikke. Denne avslåtte demoforespørselen kan brukes til å vise filtrering og ryddig oppfølging.", "declined")
        ];
    }

    private static Guid Id(int category, int number) => new($"de00000{category}-0000-4000-8000-{number:000000000000}");
}
