FROM mcr.microsoft.com/devcontainers/dotnet:1-9-bookworm

# Install PostgreSQL client tools
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends postgresql-client \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Install global .NET tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/home/vscode/.dotnet/tools"

# Switch to non-root user
USER vscode