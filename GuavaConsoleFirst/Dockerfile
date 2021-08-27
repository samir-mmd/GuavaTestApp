#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["GuavaConsoleFirst.csproj", ""]
RUN dotnet restore "./GuavaConsoleFirst.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "GuavaConsoleFirst.csproj" -c Release -o /app/build
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh

FROM build AS publish
RUN dotnet publish "GuavaConsoleFirst.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GuavaConsoleFirst.dll"]
