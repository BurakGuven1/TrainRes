using TrainReservation.Api.Services;
using TrainReservation.Api.Transport;

namespace TrainReservation.Tests;

public class ReservationServiceTests
{
    private readonly ReservationService _service = new();

    [Fact]
    public void PlanReservation_AllowsSplitAcrossWagons_WhenSeatsAvailable()
    {
        var request = new ReservationRequest(
            new Train(
                "Başkent Ekspres",
                new[]
                {
                    new Wagon("Vagon 1", 100, 68),
                    new Wagon("Vagon 2", 90, 50),
                    new Wagon("Vagon 3", 80, 80)
                }),
            RezervasyonYapilacakKisiSayisi: 3,
            KisilerFarkliVagonlaraYerlestirilebilir: true);

        var response = _service.PlanReservation(request);

        Assert.True(response.RezervasyonYapilabilir);
        Assert.Collection(
            response.YerlesimAyrinti,
            first =>
            {
                Assert.Equal("Vagon 1", first.VagonAdi);
                Assert.Equal(2, first.KisiSayisi);
            },
            second =>
            {
                Assert.Equal("Vagon 2", second.VagonAdi);
                Assert.Equal(1, second.KisiSayisi);
            });
    }

    [Fact]
    public void PlanReservation_Fails_WhenSingleWagonRequiredButNoCapacity()
    {
        var request = new ReservationRequest(
            new Train(
                "Marmara",
                new[]
                {
                    new Wagon("Vagon 1", 50, 40),
                    new Wagon("Vagon 2", 80, 60)
                }),
            RezervasyonYapilacakKisiSayisi: 10,
            KisilerFarkliVagonlaraYerlestirilebilir: false);

        var response = _service.PlanReservation(request);

        Assert.False(response.RezervasyonYapilabilir);
        Assert.Empty(response.YerlesimAyrinti);
    }
}
