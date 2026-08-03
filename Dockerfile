FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FutPib.csproj", "./"]
RUN dotnet restore "FutPib.csproj"

COPY . .
RUN dotnet publish "FutPib.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

CMD ["sh", "-c", "dotnet FutPib.dll --urls http://0.0.0.0:${PORT:-10000}"]
