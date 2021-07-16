#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-nanoserver-1809 AS build
WORKDIR /src
COPY ["MonteCristo.Web/MonteCristo.Web.csproj", "MonteCristo.Web/"]
COPY ["MonteCristo.CacheManager/MonteCristo.CacheManager.csproj", "MonteCristo.CacheManager/"]
COPY ["MonteCristo.Database/MonteCristo.Database.csproj", "MonteCristo.Database/"]
COPY ["MonteCristo.Application/MonteCristo.Application.csproj", "MonteCristo.Application/"]
RUN dotnet restore "MonteCristo.Web/MonteCristo.Web.csproj"
COPY . .
WORKDIR "/src/MonteCristo.Web"
RUN dotnet build "MonteCristo.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MonteCristo.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MonteCristo.Web.dll"]