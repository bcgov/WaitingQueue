FROM mcr.microsoft.com/dotnet/sdk:7.0 AS net-builder

# install diagnostics tools
RUN mkdir /tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump

WORKDIR /src

COPY ["Directory.Build.props", "stylecop.json", "global.json", ".editorconfig", "./"]
COPY ["Admin/Client/Admin.Client.csproj", "./Admin/Client/"]
COPY ["Admin/Common/Admin.Common.csproj", "./Admin/Common/"]
COPY ["Admin/Server/Admin.Server.csproj", "./Admin/Server/"]

RUN dotnet restore "Admin/Server/Admin.Server.csproj"
COPY . .

# Publish the main project
RUN dotnet publish "Admin/Server/Admin.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Admin.Server.dll"]
