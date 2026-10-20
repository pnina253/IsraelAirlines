FROM node:22-slim AS client
WORKDIR /client

COPY IsraelAirlines.Client/package.json IsraelAirlines.Client/package-lock.json ./
RUN npm ci

COPY IsraelAirlines.Client/ .
# Empty API URL: the client calls the API on the same origin that serves it.
ENV VITE_API_URL=
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "./IsraelAirlines/IsraelAirlines.api.csproj"
RUN dotnet publish "./IsraelAirlines/IsraelAirlines.api.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=client /client/dist ./wwwroot

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "IsraelAirlines.api.dll"]
