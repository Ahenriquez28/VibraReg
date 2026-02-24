FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RegistrationService.csproj", "./"]
RUN dotnet restore "RegistrationService.csproj"
COPY . .
RUN dotnet build "RegistrationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RegistrationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RegistrationService.dll"]