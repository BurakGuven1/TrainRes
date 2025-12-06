FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TrainReservation.Api/TrainReservation.Api.csproj", "src/TrainReservation.Api/"]
RUN dotnet restore "src/TrainReservation.Api/TrainReservation.Api.csproj"
COPY . .
WORKDIR "/src/src/TrainReservation.Api"
RUN dotnet publish "TrainReservation.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrainReservation.Api.dll"]
