ARG USER_ID=1000
ARG GROUP_ID=1000
ARG USERNAME="appuser"

FROM mcr.microsoft.com/dotnet/aspnet:8.0.8 AS base
ARG USER_ID
ARG GROUP_ID
ARG USERNAME

RUN groupadd -g ${GROUP_ID} ${USERNAME} && \
    useradd -u ${USER_ID} -g ${GROUP_ID} -m -s /bin/bash ${USERNAME} && \
    chown -R ${USERNAME}:${USERNAME} /home/${USERNAME}

# set up tailwind
RUN apt update && \
    apt install -y curl && \
    curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64 && \
    chmod +x tailwindcss-linux-x64 && \
    mv tailwindcss-linux-x64 /usr/bin/tailwindcss

FROM mcr.microsoft.com/dotnet/sdk:8.0.401 AS build
WORKDIR /app
COPY --from=base /usr/bin/tailwindcss /usr/bin/tailwindcss

COPY ["source_generators/StrongIdGenerator/StrongIdGenerator.csproj", "source_generators/StrongIdGenerator/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Persistence/Persistence.csproj", "src/Persistence/"]
COPY ["src/Client/Client.csproj", "src/Client/"]
COPY ["src/WebApi/WebApi.csproj", "src/WebApi/"]

RUN dotnet restore "source_generators/StrongIdGenerator/StrongIdGenerator.csproj"
RUN dotnet restore "src/Domain/Domain.csproj"
RUN dotnet restore "src/Application/Application.csproj"
RUN dotnet restore "src/Infrastructure/Infrastructure.csproj"
RUN dotnet restore "src/Persistence/Persistence.csproj"
RUN dotnet restore "src/Client/Client.csproj"
RUN dotnet restore "src/WebApi/WebApi.csproj"

COPY . .

RUN /usr/bin/tailwindcss \
    --input ./src/WebApi/wwwroot/app.css  \
    --output ./src/WebApi/wwwroot/app.min.css  \
    --config ./src/Client/tailwind.config.js  \
    --content "./src/Client/**/*.{cs,razor,js,css,html}" \
    --minify

WORKDIR /app/source_generators
RUN dotnet build -c Release --no-restore "StrongIdGenerator/StrongIdGenerator.csproj"

WORKDIR /app/src
RUN dotnet build -c Release --no-restore "Domain/Domain.csproj"
RUN dotnet build -c Release --no-restore "Application/Application.csproj"
RUN dotnet build -c Release --no-restore "Infrastructure/Infrastructure.csproj"
RUN dotnet build -c Release --no-restore "Persistence/Persistence.csproj"
RUN dotnet build -c Release --no-restore "Client/Client.csproj"
RUN dotnet build -c Release --no-restore "WebApi/WebApi.csproj"

FROM build AS publish
WORKDIR /app/src/
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build "WebApi/WebApi.csproj"

FROM base AS final
USER ${USERNAME}
WORKDIR /home/${USERNAME}

EXPOSE 1080
EXPOSE 1443

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s CMD curl --silent --fail https://127.0.0.1:1443/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "WebApi.dll"]
