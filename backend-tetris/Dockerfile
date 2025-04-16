# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["backend-tetris.csproj", "./"]
RUN dotnet restore "./backend-tetris.csproj"
COPY . .
RUN dotnet publish -c Release -o out

# Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "backend-tetris.dll"]