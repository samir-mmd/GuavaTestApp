#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 1783

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MockStockServer.csproj", ""]
RUN dotnet restore "./MockStockServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MockStockServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MockStockServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MockStockServer.dll"]