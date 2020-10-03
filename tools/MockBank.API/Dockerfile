FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["tools/MockBank.API/MockBank.API.csproj", "tools/MockBank.API/"]
RUN dotnet restore "tools/MockBank.API/MockBank.API.csproj"
COPY . .
WORKDIR "/src/tools/MockBank.API"
RUN dotnet build "MockBank.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MockBank.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MockBank.API.dll"]
