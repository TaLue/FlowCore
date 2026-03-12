FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/FlowCore.API/FlowCore.API.csproj", "src/FlowCore.API/"]
COPY ["src/FlowCore.Application/FlowCore.Application.csproj", "src/FlowCore.Application/"]
COPY ["src/FlowCore.Domain/FlowCore.Domain.csproj", "src/FlowCore.Domain/"]
COPY ["src/FlowCore.Infrastructure/FlowCore.Infrastructure.csproj", "src/FlowCore.Infrastructure/"]
RUN dotnet restore "src/FlowCore.API/FlowCore.API.csproj"
COPY . .
WORKDIR "/src/src/FlowCore.API"
RUN dotnet build "FlowCore.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowCore.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowCore.API.dll"]
