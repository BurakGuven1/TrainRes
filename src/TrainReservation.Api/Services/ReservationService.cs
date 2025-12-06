using TrainReservation.Api.Transport;

namespace TrainReservation.Api.Services;

public class ReservationService : IReservationService
{
    private const double OnlineOccupancyLimit = 0.7;

    public ReservationResponse PlanReservation(ReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Tren?.Vagonlar is null || request.Tren.Vagonlar.Count == 0)
        {
            return new ReservationResponse(false, Array.Empty<Placement>());
        }

        if (request.RezervasyonYapilacakKisiSayisi <= 0)
        {
            return new ReservationResponse(false, Array.Empty<Placement>());
        }

        var vagons = request.Tren.Vagonlar
            .Select(vagon => new vagonWithAvailability(vagon, CalculateAvailableSeats(vagon)))
            .Where(vagon => vagon.AvailableSeats > 0)
            .ToList();

        if (vagons.Count == 0)
        {
            return new ReservationResponse(false, Array.Empty<Placement>());
        }

        if (!request.KisilerFarkliVagonlaraYerlestirilebilir)
        {
            var singlevagon = vagons
                .Where(vagon => vagon.AvailableSeats >= request.RezervasyonYapilacakKisiSayisi)
                .OrderBy(vagon => vagon.AvailableSeats)
                .FirstOrDefault();

            if (singlevagon is null)
            {
                return new ReservationResponse(false, Array.Empty<Placement>());
            }

            var placement = new Placement(singlevagon.vagon.Ad, request.RezervasyonYapilacakKisiSayisi);
            return new ReservationResponse(true, new[] { placement });
        }

        var (success, placements) = AllocateAcrossvagons(request.RezervasyonYapilacakKisiSayisi, vagons);
        return new ReservationResponse(success, success ? placements : Array.Empty<Placement>());
    }

    private static (bool Success, IReadOnlyCollection<Placement> Placements) AllocateAcrossvagons(
        int requestedSeats,
        IEnumerable<vagonWithAvailability> vagons)
    {
        var remaining = requestedSeats;
        var placements = new List<Placement>();

        foreach (var vagon in vagons)
        {
            if (remaining == 0)
            {
                break;
            }

            var seatsToUse = Math.Min(vagon.AvailableSeats, remaining);
            if (seatsToUse <= 0)
            {
                continue;
            }

            placements.Add(new Placement(vagon.vagon.Ad, seatsToUse));
            remaining -= seatsToUse;
        }

        return (remaining == 0, placements);
    }

    private static int CalculateAvailableSeats(Wagon vagon)
    {
        if (vagon.Kapasite <= 0)
        {
            return 0;
        }

        var limit = (int)Math.Floor(vagon.Kapasite * OnlineOccupancyLimit);
        var available = limit - vagon.DoluKoltukAdet;
        return Math.Max(0, available);
    }

    private sealed record vagonWithAvailability(Wagon vagon, int AvailableSeats);
}
