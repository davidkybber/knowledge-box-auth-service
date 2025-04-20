# Docker Development Setup

This guide explains how to run the Knowledge Box Auth Service locally using Docker Compose.

## Prerequisites

- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)

## Getting Started

The `docker-compose.yml` file sets up:
- A PostgreSQL database
- The Auth Service connected to the database

### Start the Services

Run the following command:

```bash
./start-local.sh
```

Or manually with:

```bash
docker-compose up --build
```

### Access the Services

- Auth Service: http://localhost:8080
- PostgreSQL: localhost:5432
  - Username: postgres
  - Password: postgres
  - Database: knowledgebox

### Stop the Services

Press `Ctrl+C` in the terminal where the services are running, or run:

```bash
docker-compose down
```

### View Logs

```bash
docker-compose logs -f
```

### Troubleshooting

If you have issues with port conflicts:
1. Check if you have PostgreSQL running locally already
2. Modify the ports in `docker-compose.yml`

## Persistent Data

The PostgreSQL data is stored in a Docker volume named `knowledge-box-postgres-data`. This ensures your data persists between container restarts.

To reset all data:

```bash
docker-compose down -v
```

## Running Database Migrations

The Auth Service automatically applies migrations at startup. If you need to manually apply migrations, use:

```bash
docker-compose exec auth-service dotnet ef database update
```

## Environment Variables

You can customize the configuration by modifying the environment variables in `docker-compose.yml`.

## Production Deployment

For production, use the Dockerfile without Docker Compose. See the deployment workflow in `.github/workflows/deploy.yml`. 