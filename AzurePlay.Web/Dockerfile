FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["AzurePlay.Web/AzurePlay.Web.csproj", "AzurePlay.Web/"]
RUN dotnet restore "AzurePlay.Web/AzurePlay.Web.csproj"
COPY . .
WORKDIR "/src/AzurePlay.Web"
RUN dotnet build "AzurePlay.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzurePlay.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzurePlay.Web.dll"]