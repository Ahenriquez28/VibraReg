FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5089

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JwtAuthApi.csproj", "./"]
RUN dotnet restore "JwtAuthApi.csproj"
COPY . .
RUN dotnet build "JwtAuthApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JwtAuthApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JwtAuthApi.dll"]