#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["micro_service_report_worker/micro_service_report_worker.csproj", "micro_service_report_worker/"]
COPY ["micro_service_shared/micro_service_shared.csproj", "micro_service_shared/"]
RUN dotnet restore "micro_service_report_worker/micro_service_report_worker.csproj"
COPY . .
WORKDIR "/src/micro_service_report_worker"
RUN dotnet build "micro_service_report_worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "micro_service_report_worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "micro_service_report_worker.dll", "--environment=Release"]
