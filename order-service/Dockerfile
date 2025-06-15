FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY order-service/OrderService.csproj order-service/
COPY ShopOnline.Shared/ShopOnline.Shared.csproj ShopOnline.Shared/
RUN dotnet restore order-service/OrderService.csproj
COPY . .
WORKDIR /src/order-service
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
