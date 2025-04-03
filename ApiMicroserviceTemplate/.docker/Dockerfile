#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8088
EXPOSE 443

ARG ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT ${ENVIRONMENT}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ["/src/Api/Api.csproj", "Api/"]

COPY ["NuGet.Config","NuGet.Config"]

COPY /src/Api Api/

ARG GIT_TOKEN
ARG GIT_USER

RUN sed -i "s|</configuration>|<packageSourceCredentials><Github><add key=\"Username\" value=\"${GIT_USER}\" /><add key=\"ClearTextPassword\" value=\"${GIT_TOKEN}\" /></Github></packageSourceCredentials></configuration>|" NuGet.Config
RUN cat NuGet.Config

RUN dotnet restore --configfile "./NuGet.Config" "/source/Api/Api.csproj"

WORKDIR "/source/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish --no-restore

FROM base AS final

ARG ENVIRONMENT_NAME
ENV ENVIRONMENT_NAME ${ENVIRONMENT_NAME}
RUN apt-get update && apt-get install -y procps && rm -rf /var/lib/apt/lists/*
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y newrelic-dotnet-agent
    
ARG NEW_RELIC_KEY
ARG APM_NAME
RUN echo $APM_NAME

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=$NEW_RELIC_KEY \
NEW_RELIC_APP_NAME=$APM_NAME

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
