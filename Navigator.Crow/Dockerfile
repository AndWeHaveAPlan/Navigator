#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

ARG C2M_NUGET_GROUP_URL
ENV C2M_NUGET_GROUP_URL $C2M_NUGET_GROUP_URL

WORKDIR /src
COPY ["Navigator.Crow/Navigator.Crow.csproj", "Navigator.Crow/"]
COPY ["Navigator.Core/Navigator.Core.csproj", "Navigator.Core/"]
RUN echo $C2M_NUGET_GROUP_URL
RUN dotnet restore "Navigator.Crow/Navigator.Crow.csproj" -s $C2M_NUGET_GROUP_URL
COPY . .
WORKDIR "/src/Navigator.Crow"
RUN dotnet build "Navigator.Crow.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Navigator.Crow.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Navigator.Crow.dll"]