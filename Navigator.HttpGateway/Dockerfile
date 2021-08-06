#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

ARG C2M_NUGET_GROUP_URL
ENV C2M_NUGET_GROUP_URL $C2M_NUGET_GROUP_URL

WORKDIR /src
COPY ["Navigator.HttpGateway/Navigator.HttpGateway.csproj", "Navigator.HttpGateway/"]
COPY ["Navigator.Core/Navigator.Core.csproj", "Navigator.Core/"]
RUN dotnet restore "Navigator.HttpGateway/Navigator.HttpGateway.csproj" -s $C2M_NUGET_GROUP_URL
COPY . .
WORKDIR "/src/Navigator.HttpGateway"
RUN dotnet build "Navigator.HttpGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Navigator.HttpGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Navigator.HttpGateway.dll"]