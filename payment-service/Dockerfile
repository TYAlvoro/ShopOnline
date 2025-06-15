FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY payment-service/PaymentService.csproj payment-service/
COPY ShopOnline.Shared/ShopOnline.Shared.csproj ShopOnline.Shared/
RUN dotnet restore payment-service/PaymentService.csproj
COPY . .
WORKDIR /src/payment-service
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
