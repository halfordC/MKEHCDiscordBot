FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

#Copy csproj and restore as distinct layers
COPY *.csproj .
COPY NuGet.Config .
RUN dotnet restore

#Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

#Build runitme image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app/out .

COPY config.json .
ENTRYPOINT ["dotnet", "MKEHCBot.dll"]