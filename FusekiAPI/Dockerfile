#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FusekiAPI/FusekiAPI.csproj", "FusekiAPI/"]
COPY ["MessageService/MessageService.csproj", "MessageService/"]
RUN dotnet restore "FusekiAPI/FusekiAPI.csproj"
COPY . .
WORKDIR "/src/FusekiAPI"
RUN dotnet build "FusekiAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FusekiAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FusekiAPI.dll"]