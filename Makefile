.PHONY: test coverage format dev-up dev-down dev-logs dev-logs-db dev-logs-seq dev-status dev-clean api

# Docker Compose commands using devcontainer configuration
COMPOSE_FILE = .devcontainer/docker-compose.yml
COMPOSE_PROJECT = teslagotchi-dev

# Start development services (PostgreSQL and Seq)
dev-up:
	@echo "🚀 Starting development services..."
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) up -d postgres seq
	@echo "✅ PostgreSQL is running on localhost:5432"
	@echo "✅ Seq is running on http://localhost:8081"

# Stop development services
dev-down:
	@echo "🛑 Stopping development services..."
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) down
	@echo "✅ Development services stopped"

# View logs for development services
dev-logs:
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) logs -f

# View logs for PostgreSQL only
dev-logs-db:
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) logs -f postgres

# View logs for Seq only
dev-logs-seq:
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) logs -f seq

# Check status of development services
dev-status:
	@echo "🔍 Checking development services status..."
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) ps
	@echo ""
	@echo "🔍 Checking what's using port 5432..."
	@lsof -i :5432 2>/dev/null || echo "Port 5432 is available"

# Clean development data (WARNING: This will delete all data!)
dev-clean:
	@echo "🧹 Cleaning development data..."
	@docker-compose -f $(COMPOSE_FILE) -p $(COMPOSE_PROJECT) down -v
	@echo "✅ Development data cleaned"

# Run API with database
api: dev-up
	@echo "🚀 Starting Teslagotchi API..."
	@cd src/apps/api/Teslagotchi.Api && dotnet run

# Run tests with coverage and generate reports
coverage:
	@echo "🧹 Cleaning old test results..."
	@rm -rf TestResults/
	@rm -rf coverage/
	@find . -name "*coverage*" -type f -delete
	@echo "🧪 Running tests with coverage..."
	@dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
	@echo "📊 Generating coverage reports..."
	@reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:"Html;lcov;OpenCover"
	@echo "✅ Coverage reports ready:"
	@echo "   - HTML: coverage/index.html"
	@echo "   - LCOV: coverage/lcov.info (for IDE gutters)"
	@echo "   - OpenCover: coverage/OpenCover.xml (for Rider)"

# Just run tests
test:
	@dotnet test 

# Format codebase
format:
	@dotnet format --no-restore