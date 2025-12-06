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

        var wagons = request.Tren.Vagonlar
            .Select(wagon => new WagonWithAvailability(wagon, CalculateAvailableSeats(wagon)))
            .Where(wagon => wagon.AvailableSeats > 0)
            .ToList();

        if (wagons.Count == 0)
        {
            return new ReservationResponse(false, Array.Empty<Placement>());
        }

        if (!request.KisilerFarkliVagonlaraYerlestirilebilir)
        {
            var singleWagon = wagons
                .Where(wagon => wagon.AvailableSeats >= request.RezervasyonYapilacakKisiSayisi)
                .OrderBy(wagon => wagon.AvailableSeats)
                .FirstOrDefault();

            if (singleWagon is null)
            {
                return new ReservationResponse(false, Array.Empty<Placement>());
            }

            var placement = new Placement(singleWagon.Wagon.Ad, request.RezervasyonYapilacakKisiSayisi);
            return new ReservationResponse(true, new[] { placement });
        }

        var (success, placements) = AllocateAcrossWagons(request.RezervasyonYapilacakKisiSayisi, wagons);
        return new ReservationResponse(success, success ? placements : Array.Empty<Placement>());
    }

    private static (bool Success, IReadOnlyCollection<Placement> Placements) AllocateAcrossWagons(
        int requestedSeats,
        IEnumerable<WagonWithAvailability> wagons)
    {
        var remaining = requestedSeats;
        var placements = new List<Placement>();

        foreach (var wagon in wagons)
        {
            if (remaining == 0)
            {
                break;
            }

            var seatsToUse = Math.Min(wagon.AvailableSeats, remaining);
            if (seatsToUse <= 0)
            {
                continue;
            }

            placements.Add(new Placement(wagon.Wagon.Ad, seatsToUse));
            remaining -= seatsToUse;
        }

        return (remaining == 0, placements);
    }

    private static int CalculateAvailableSeats(Wagon wagon)
    {
        if (wagon.Kapasite <= 0)
        {
            return 0;
        }

        var limit = (int)Math.Floor(wagon.Kapasite * OnlineOccupancyLimit);
        var available = limit - wagon.DoluKoltukAdet;
        return Math.Max(0, available);
    }

    private sealed record WagonWithAvailability(Wagon Wagon, int AvailableSeats);
}
