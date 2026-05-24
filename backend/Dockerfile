FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Verdiq.slnx .
COPY Verdiq.Domain/Verdiq.Domain.csproj Verdiq.Domain/
COPY Verdiq.Application/Verdiq.Application.csproj Verdiq.Application/
COPY Verdiq.Infrastructure/Verdiq.Infrastructure.csproj Verdiq.Infrastructure/
COPY Verdiq.API/Verdiq.API.csproj Verdiq.API/

RUN dotnet restore Verdiq.API/Verdiq.API.csproj

COPY . .
RUN dotnet publish Verdiq.API/Verdiq.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Verdiq.API.dll"]
