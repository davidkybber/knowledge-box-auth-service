# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/KnowledgeBox.Auth.csproj", "src/"]
RUN dotnet restore "src/KnowledgeBox.Auth.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "src/KnowledgeBox.Auth.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "KnowledgeBox.Auth.dll"] 