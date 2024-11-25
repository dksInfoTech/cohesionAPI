# Stage 1: Build
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
#WORKDIR /app
#
## Copy everything and restore dependencies for the entire solution
#COPY . .
#RUN dotnet restore Product.sln
#
## Publish the startup project (Product.Web) to the /out directory
#RUN dotnet publish -c Release -o /out Product.Web/Product.Web.csproj
#
## Stage 2: Runtime
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
#WORKDIR /app
#
## Copy only the published files from the build stage
#COPY --from=build-env /out .
#
## Set environment variable to listen on port 80
#ENV ASPNETCORE_URLS=http://+:80
#
## Expose port 80
#EXPOSE 80
#
## Start the application
#ENTRYPOINT ["dotnet", "Product.Web.dll"]