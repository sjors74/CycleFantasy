FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish WebCycleManager/WebCycleManager.csproj \
    -c Release \
    -o /app/publish

# Runtime image
FROM mcr.microsoft.com/playwright/dotnet:v1.56.0-jammy

# Install .NET 10 runtime
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin \
    --install-dir /usr/share/dotnet \
    --channel 10.0

WORKDIR /app

COPY --from=build /app/publish .

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV ASPNETCORE_URLS=http://+:8080

# EXPLICIET browsers installeren
RUN pwsh playwright.ps1 install chromium

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebCycleManager.dll"]