#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PaymentGatewayAPI/WebAPI.csproj", "PaymentGatewayAPI/"]
COPY ["PaymentGateway.Data/Infrastructure.csproj", "PaymentGateway.Data/"]
COPY ["Core/Application.csproj", "Core/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "PaymentGatewayAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/PaymentGatewayAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]
