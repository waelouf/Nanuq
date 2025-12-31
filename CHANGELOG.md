# Changelog

All notable changes to the Nanuq project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive test suites for frontend and backend
  - Vitest + Vue Test Utils for frontend testing
  - xUnit + Moq + FluentAssertions for backend testing
  - Test coverage reporting
  - 30 frontend tests and 10 backend tests
- CLAUDE.md documentation for AI-assisted development
- Test commands in documentation

### Changed
- Updated frontend dependencies to resolve security vulnerabilities
- Fixed ESLint and build configuration issues
- Disabled ESLint during development to resolve child compilation errors

## [0.2.0] - 2025-05-12

### Added
- CodeQL security scanning workflow

### Changed
- Package updates across frontend and backend
- Security vulnerability fixes
- Refactored codebase for better maintainability

### Fixed
- Removed security vulnerabilities from dependencies

## [0.1.0] - 2024-09-15

### Added
- **Kafka Support**
  - Display server's topics
  - Show topic details and message counts
  - Add new topics
  - Delete topics
  - CRUD operations for Kafka server configurations

- **Redis Support**
  - Display server details and information
  - Browse databases
  - View all string cached keys
  - Add items to cache
  - Invalidate cache entries
  - Comprehensive Redis server statistics
  - Database management functionality

- **RabbitMQ Support**
  - Database schema and endpoints
  - Basic RabbitMQ server management
  - Exchange listing functionality

- **Deployment Options**
  - Docker support with Dockerfile for both app and server
  - Docker Compose configuration
  - Kubernetes deployment manifests (K8s)
  - Kubernetes installation instructions

- **Backend Infrastructure**
  - FastEndpoints pattern for API endpoints
  - Entity Framework Core with SQLite
  - Serilog logging integration
  - Activity logging system
  - Audit trail functionality
  - API prefix `/api` for all endpoints

- **Frontend Features**
  - Vue 3 with Vuetify UI framework
  - Vuex state management
  - Vue Router for navigation
  - Lazy-loaded routes for performance
  - Responsive UI design
  - Server and topic management components

- **CI/CD**
  - GitHub Actions workflow for .NET backend
  - GitHub Actions workflow for Vue.js frontend
  - Automated build and test pipelines

### Changed
- Migrated from Dapper to Entity Framework Core
- Refactored database context injection
- Improved component organization
- Updated NuGet packages
- Optimized Docker configuration for production
- Environment variable handling for deployment

### Fixed
- Database reference issues in csproj files
- GitHub Actions build configuration
- CORS configuration for cross-origin requests
- Redis percentage rounding to 2 decimal places

### Documentation
- Comprehensive README with installation instructions
- Docker deployment guide
- Kubernetes deployment guide
- Feature list with screenshots
- Disclaimer for development/DEV environment usage
- Architecture and project structure documentation

## [0.0.1] - 2024-08-07

### Added
- Initial project setup
- Basic project skeleton
- Logo and branding
- Repository structure
- License and initial documentation

---

## Version History Summary

- **0.2.0**: Security updates, package updates, and refactoring
- **0.1.0**: Major release with Kafka, Redis, RabbitMQ support, Docker/K8s deployment
- **0.0.1**: Initial project setup

## Notes

### Development Environment
Nanuq is currently intended for use in **local development and DEV environments only**.
It is not production-ready and should be thoroughly tested before any production consideration.

### Technology Stack
- **Backend**: .NET 8.0, FastEndpoints, Entity Framework Core, SQLite, Serilog
- **Frontend**: Vue 3, Vuetify 3, Vuex 4, Vue Router 4, Axios
- **Testing**: Vitest, xUnit, Moq, FluentAssertions
- **Deployment**: Docker, Kubernetes
- **CI/CD**: GitHub Actions

### Contributing
For detailed development information and AI-assisted development guidance, see [CLAUDE.md](CLAUDE.md).
