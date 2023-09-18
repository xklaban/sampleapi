FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /build
RUN mkdir -p /build/dist

COPY . . 

RUN dotnet restore
RUN dotnet publish -c Release -o ./dist

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime

WORKDIR /srv/sampleapi

COPY --from=build-env /build/dist .

ENTRYPOINT ["dotnet", "sampleapi.dll"]
