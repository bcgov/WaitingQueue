FROM mcr.microsoft.com/dotnet/sdk:7.0 AS net-builder

# install diagnostics tools
RUN mkdir /tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump

WORKDIR /src

COPY ["Directory.Build.props", "stylecop.json", "global.json", ".editorconfig", "./"]
COPY ["WaitingQueueWeb/WaitingQueueWeb.csproj", "./WaitingQueueWeb/"]
COPY ["WebCommon/WebCommon.csproj", "./WebCommon/"]
COPY ["TicketManagement/TicketManagement.csproj", "./TicketManagement/"]

RUN dotnet restore "WaitingQueueWeb/WaitingQueueWeb.csproj"
COPY . .

# Publish the main project
RUN dotnet publish "WaitingQueueWeb/WaitingQueueWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Debian image
#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# RH image
FROM registry.access.redhat.com/ubi8/dotnet-70-runtime AS final
ARG VERSION
ENV VERSION=$VERSION
ENV ASPNETCORE_URLS=http://*:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# copy diagnostics tools
WORKDIR /tools
COPY --from=net-builder /tools .

# copy app
WORKDIR /app
COPY --from=net-builder /app/publish .

ENTRYPOINT ["dotnet", "WaitingQueueWeb.dll"]

