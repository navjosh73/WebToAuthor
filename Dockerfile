# Stage 1: Build the application (Uses the large SDK image)
###FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
###WORKDIR /src
###COPY ~/.nuget/packages /root/.nuget/packages
# Copy the project file and restore dependencies
###COPY *.csproj .
###RUN msbuild ./WebToAuthorise.csproj -t:Restore /p:RestoreDisableParallel=true
# Copy everything else and publish the application
###COPY . .
###RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the final image (Uses the small Runtime image)
###FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
###WORKDIR /app
# Copy published output from the 'build' stage
###COPY --from=build /app/publish .
# Define the internal port Kestrel will listen on
###ENV ASPNETCORE_URLS=http://+:8080
###EXPOSE 8080
# Define the entry point, running your compiled DLL
###ENTRYPOINT ["dotnet", "WebToAuthorise.dll"] 
# NOTE: Replace "webtoauthor.dll" with the actual name of your compiled DLL




# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=1
RUN dotnet restore --disable-parallel ./WebToAuthorise.csproj
RUN dotnet publish ./WebToAuthorise.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebToAuthorise.dll"]
