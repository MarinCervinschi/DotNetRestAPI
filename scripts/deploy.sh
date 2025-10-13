#!/bin/bash

# Automated deployment script for DotNetRestAPI
# Following styleGuide.md for consistency

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Utility functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker not found. Please install Docker Desktop."
        exit 1
    fi

    if ! command -v docker compose &> /dev/null; then
        log_error "Docker Compose not found."
        exit 1
    fi

    if [ ! -f ".env" ]; then
        log_warn ".env file not found. Copying from .env.example..."
        cp .env.example .env
        log_warn "Please modify .env with correct credentials before continuing."
        exit 1
    fi

    log_info "Prerequisites verified ✓"
}

# Build application
build_app() {
    log_info "Building application..."
    docker compose build --no-cache dotnetrestapi
    log_info "Build completed ✓"
}

# Start services
start_services() {
    log_info "Starting services..."
    docker compose up -d

    # Wait for services to be ready
    log_info "Waiting for services to be ready..."
    sleep 10

    # Check service status more intelligently
    log_info "Checking service status..."

    # Get status of all services
    status_output=$(docker compose ps)
    echo "$status_output"

    # Check if postgres is healthy (it has health checks)
    if echo "$status_output" | grep -q "dotnetapi_postgres.*Up.*healthy"; then
        log_info "✅ PostgreSQL is healthy"
    else
        log_error "❌ PostgreSQL is not healthy"
        docker compose logs postgres
        exit 1
    fi

    # Check if dotnet app is running (it doesn't have health checks, just check if Up)
    if echo "$status_output" | grep -q "dotnetapi_app.*Up"; then
        log_info "✅ .NET API is running"
    else
        log_error "❌ .NET API is not running"
        docker compose logs dotnetrestapi
        exit 1
    fi

    # Additional check: try to connect to the API
    log_info "Testing API connectivity..."
    sleep 5  # Give it a bit more time

    if curl -s -f http://localhost:5131/health >/dev/null 2>&1; then
        log_info "✅ API health endpoint responding"
    elif curl -s -f http://localhost:5131/ >/dev/null 2>&1; then
        log_info "✅ API is responding (no health endpoint configured)"
    else
        log_warn "⚠️  API not responding yet, but containers are up. Check logs if needed."
    fi

    log_info "Services started successfully ✓"
    log_info "API available at: http://localhost:5131"
    log_info "Swagger UI: http://localhost:5131/swagger"
}

# Stop services
stop_services() {
    log_info "Stopping services..."
    docker compose down
    log_info "Services stopped ✓"
}

# Complete reset (warning: deletes data)
reset_all() {
    log_warn "WARNING: This operation will delete all database data!"
    read -p "Continue? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        log_info "Performing complete environment reset..."
        docker compose down -v
        docker system prune -f
        log_info "Reset completed ✓"
    else
        log_info "Reset cancelled."
    fi
}

# Main menu
show_menu() {
    echo
    echo "=== DotNetRestAPI Deployment Script ==="
    echo "1. Build application"
    echo "2. Start services"
    echo "3. Stop services"
    echo "4. Restart services"
    echo "5. View logs"
    echo "6. Complete reset (DELETES DATA)"
    echo "7. Services status"
    echo "0. Exit"
    echo
}

# Handle menu options
handle_option() {
    case $1 in
        1)
            build_app
            ;;
        2)
            start_services
            ;;
        3)
            stop_services
            ;;
        4)
            stop_services
            build_app
            start_services
            ;;
        5)
            docker compose logs -f
            ;;
        6)
            reset_all
            ;;
        7)
            docker compose ps
            ;;
        0)
            log_info "Goodbye!"
            exit 0
            ;;
        *)
            log_error "Invalid option"
            ;;
    esac
}

# Main function
main() {
    check_prerequisites

    if [ $# -eq 0 ]; then
        # Interactive mode
        while true; do
            show_menu
            read -p "Select an option: " choice
            handle_option $choice
        done
    else
        # Command line mode
        handle_option $1
    fi
}

# Execute script
main "$@"
