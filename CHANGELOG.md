# Changelog

All notable changes to the Nanuq project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-01-01

**Major Release** - Production-ready frontend with comprehensive testing, Docker optimization, and significant performance improvements.

### Added
- Comprehensive test suites for frontend and backend
  - Vitest + Vue Test Utils for frontend testing (29 tests passing)
  - xUnit + Moq + FluentAssertions for backend testing (10 tests)
  - Test coverage reporting with v8 provider
  - Automated test commands in package.json
- **Documentation**
  - CLAUDE.md for AI-assisted development guidance
  - Docker deployment documentation (Docker/README.md)
  - Frontend performance optimization guide (PERFORMANCE.md)
- **Docker & Infrastructure**
  - Nginx reverse proxy configuration in frontend Docker container
  - Multi-stage Docker build for optimized production images
  - Docker networking configuration for frontend-backend communication
- **Frontend Components**
  - VSheet component added to Vuetify imports (was missing)

### Changed
- **Version**: Updated application version from 0.2.0 to 1.0.0
- **CI/CD**
  - GitHub Actions: Updated Node.js from 18.x to 20.x (LTS)
  - GitHub Actions: Added package-lock.json to repository for consistent builds
  - Fixed npm ci sync errors with picomatch and yaml versions
- **UI/UX Improvements**
  - Text field widths in Add modals: 300px → 75% (better usability)
  - Responsive text fields adapt to dialog width
- **Docker Architecture**
  - Frontend container: http-server → nginx for production serving
  - API calls proxied through nginx to backend service
  - Backend exposed only to internal Docker network (not to host)
  - Updated docker-compose.yml with proper service configuration
- **Frontend Performance**
  - HomePage lazy-loaded for smaller initial bundle
  - Bootstrap CSS bundled locally (removed CDN dependency)
  - Script loading optimized (removed unnecessary defer attributes)
  - External resources use preconnect for faster DNS resolution
- **Vuetify**
  - Replaced deprecated components: VTabsItems → VTabsWindow, VTabItem → VTabsWindowItem
  - Fixed component imports and build warnings
- **Build Configuration**
  - Updated ESLint configuration (disabled during development)
  - Webpack optimizations: usedExports, concatenateModules
  - Script loading mode set to 'blocking' for faster initial render

### Fixed
- **GitHub Actions**
  - Frontend workflow failing due to missing package-lock.json
  - npm ci package sync errors
- **Tests**
  - All 19 failing frontend tests (apiClient, sqlite store, redis store)
  - Vuetify component mocking issues in test suites
  - API endpoint tests aligned with actual implementation
- **Docker**
  - Frontend-to-backend communication (browser couldn't resolve internal hostnames)
  - CORS configuration for proper API proxying
  - nginx proxy configuration for API requests
- **UI/Styling**
  - Broken styling in production build (CSS loading order)
  - Critical CSS now loads synchronously
  - Production build white screen on initial load
- **Build**
  - Vuetify component import warnings (VTabsItems, VTabItem)
  - Removed defer from Vue app scripts causing mount delays
  - Fixed CDN redirect delay for external resources

### Performance
- **Build Time**: Reduced from 116s to 48s (58% faster!)
- **Bundle Optimization**
  - app.js: 16.67 KiB → 14.94 KiB
  - HomePage: Extracted to separate 2.04 KiB chunk (lazy-loaded)
  - Improved tree-shaking and dead code elimination
- **Runtime Performance**
  - No external CSS requests (bundled locally)
  - No render-blocking external resources
  - Faster DNS resolution via preconnect hints
  - Eliminated white screen on initial page load
  - Immediate Vue app initialization

### Security
- Updated frontend dependencies to resolve vulnerabilities
- Single-origin architecture (better CSP compliance)
- No external CDN dependencies for critical resources

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

- **1.0.0**: Production-ready release with comprehensive testing, Docker optimization, and performance improvements
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
