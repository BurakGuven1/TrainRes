using TrainReservation.Api.Transport;

namespace TrainReservation.Api.Services;

public interface IReservationService
{
    ReservationResponse PlanReservation(ReservationRequest request);
}
