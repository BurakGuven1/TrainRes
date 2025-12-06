namespace TrainReservation.Api.Transport;

public record ReservationRequest(Train Tren, int RezervasyonYapilacakKisiSayisi, bool KisilerFarkliVagonlaraYerlestirilebilir);

public record Train(string Ad, IReadOnlyCollection<Wagon> Vagonlar);

public record Wagon(string Ad, int Kapasite, int DoluKoltukAdet);

public record ReservationResponse(bool RezervasyonYapilabilir, IReadOnlyCollection<Placement> YerlesimAyrinti);

public record Placement(string VagonAdi, int KisiSayisi);
