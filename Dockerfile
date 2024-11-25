#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5174
ENV ASPNETCORE_URLS=http://*:5174

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src

COPY ["Product.Web/Product.Web.csproj", "Product.Web/"]
COPY ["Product.Dal/Product.Dal.csproj", "Product.Dal/"]
COPY ["Product.Bal/Product.Bal.csproj", "Product.Bal/"]
COPY ["Product.Dapper.Lib/Product.Dapper.Lib.csproj", "Product.Dapper.Lib/"]
RUN dotnet restore "Product.Web/Product.Web.csproj"
COPY . .
WORKDIR "/src/Product.Web"
RUN dotnet build "Product.Web.csproj" -c Development -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
ARG ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=Development
RUN dotnet publish "./Product.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.Web.dll"]