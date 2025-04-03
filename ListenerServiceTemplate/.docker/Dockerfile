FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443
ARG ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT ${ENVIRONMENT}
ENV DOTNET_ENVIRONMENT ${ENVIRONMENT}

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl											 
COPY ["src/Listener.csproj", "src/"]

COPY ["NuGet.Config","NuGet.Config"]

ARG GIT_TOKEN
ARG GIT_USER

RUN sed -i "s|</configuration>|<packageSourceCredentials><Github><add key=\"Username\" value=\"${GIT_USER}\" /><add key=\"ClearTextPassword\" value=\"${GIT_TOKEN}\" /></Github></packageSourceCredentials></configuration>|" NuGet.Config

RUN cat NuGet.Config

RUN dotnet restore --configfile "./NuGet.Config" "src/Listener.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "Listener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Listener.csproj" -c Release -o /app/publish

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

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=$NEW_RELIC_KEY \
NEW_RELIC_APP_NAME=$APM_NAME

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Listener.dll"]
