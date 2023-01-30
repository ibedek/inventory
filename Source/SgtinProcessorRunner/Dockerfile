FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Source/SgtinProcessorRunner/SgtinProcessorRunner.csproj", "Source/SgtinProcessorRunner/"]
RUN dotnet restore "Source/SgtinProcessorRunner/SgtinProcessorRunner.csproj"
COPY . .
WORKDIR "/src/Source/SgtinProcessorRunner"
RUN dotnet build "SgtinProcessorRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SgtinProcessorRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SgtinProcessorRunner.dll"]
