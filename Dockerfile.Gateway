FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["src/Checkout.Gateway.API/Checkout.Gateway.API.csproj", "src/Checkout.Gateway.API/"]
RUN dotnet restore "src/Checkout.Gateway.API/Checkout.Gateway.API.csproj"
COPY . .
WORKDIR "/src/src/Checkout.Gateway.API"
RUN dotnet build "Checkout.Gateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Checkout.Gateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Checkout.Gateway.API.dll"]
