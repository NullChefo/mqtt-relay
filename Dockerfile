FROM mcr.microsoft.com/dotnet/sdk:8.0  AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["MqttRelay.csproj", "MqttRelay/"]
RUN dotnet restore "MqttRelay/MqttRelay.csproj"

WORKDIR "/src/MqttRelay"
COPY . .
RUN dotnet build "MqttRelay.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

#: Self-contained applications are required to use the application host. Either set SelfContained to false or set UseAppHost to true.
# /p:UseAppHost=false

#
# -p:PublishAot=true

# Timezones disabled
#/p:InvariantGlobalization=true
RUN dotnet publish "MqttRelay.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-musl-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:DeleteExistingFiles=true  /p:TrimUnusedDependencies=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:StripSymbols=false



FROM alpine:3.16

USER $APP_UID
#WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apk update && apk upgrade && \
    apk add --no-cache ca-certificates krb5-libs libgcc libintl libssl1.1 zlib libstdc++ lttng-ust tzdata userspace-rcu

# Create a non-root user to run the application
RUN adduser -D -u 1001 myuser
USER myuser

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Set the invariant mode since icu_libs isn't included (see https://github.com/dotnet/announcements/issues/20)
#ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./MqttRelay"]



# TODO make this work


#
#FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-jammy-chiseled-extra
## 👇 set to use the non-root USER here
##USER $APP_UID
#EXPOSE 8080
#EXPOSE 8081
#ENV DOTNET_RUNNING_IN_CONTAINER=true
#ENV ASPNETCORE_ENVIRONMENT=Production
## 👇 make http to be on port 80
##ENV ASPNETCORE_HTTP_PORTS=80
#
#COPY --from=publish /app/publish /app
#
#ENTRYPOINT ["./app/MqttRelay"]
