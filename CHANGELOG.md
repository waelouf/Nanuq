# Changelog

All notable changes to the Nanuq project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Dashboard Enhancements**
  - AWS metrics card displaying server count and resource count (SQS + SNS)
  - AWS environment breakdown in dashboard card (Development, Staging, Production)
  - AWS Quick Action button for adding new AWS servers
  - Azure metrics card displaying server count and resource count (Queues + Topics)
  - Azure environment breakdown in dashboard card
  - Azure Quick Action button for adding new Azure servers
  - Dashboard grid updated to support 5 platforms (Kafka, Redis, RabbitMQ, AWS, Azure)
  - Activity Log widget now displays actual activity descriptions (fixed log.message → log.log)
  - Recent Activity widget properly shows relative timestamps
  - AWS resource counting across all configured servers
  - Azure resource counting across all configured servers
  - Refresh functionality for AWS and Azure metrics

- **Azure Service Bus Support (Complete Implementation)**
  - Full integration with Azure Service Bus for queues, topics, and subscriptions
  - Backend: New Nanuq.Azure class library project with modular structure
    - Nanuq.Azure/ServiceBus/ - Complete Service Bus implementation
    - Azure.Messaging.ServiceBus SDK v7.18.1
  - Backend: ServiceBusRepository with 14 methods (GetQueuesAsync, CreateQueueAsync, DeleteQueueAsync, SendMessageAsync, ReceiveMessagesAsync, GetTopicsAsync, CreateTopicAsync, DeleteTopicAsync, PublishMessageAsync, GetSubscriptionsAsync, CreateSubscriptionAsync, DeleteSubscriptionAsync, GetQueueDetailsAsync, GetTopicDetailsAsync)
  - Backend: 17 Azure operation endpoints (5 queue + 7 topic/subscription + 5 server config) with credential auto-detection
  - Backend: Database migration (008_CreateAzureTables.sql) creates azure_servers table
  - Backend: Database migration (009_AddAzureActivityTypes.sql) adds 10 Azure activity types
  - Backend: Extended ActivityTypeEnum with 10 new Azure activity types (32-41)
  - Backend: Extended ServerType enum with Azure value
  - Backend: AzureRepository with audit logging for add/remove server operations
  - Backend: AzureRecord entity for server configuration storage
  - Backend: Type-safe request DTOs (CreateQueueRequest, CreateTopicRequest, CreateSubscriptionRequest, SendMessageRequest, PublishMessageRequest)
  - Backend: ServerCredentials integration for secure Azure connection string storage
  - Frontend: Complete Azure management UI with 10 Vue components
  - Frontend: ListServers.vue with namespace, region, environment display and add/delete functionality
  - Frontend: AddServer.vue with two-tab interface (Server Details | Credentials) and 30+ Azure region dropdown
  - Frontend: ManageAzure.vue tabbed interface (Queues | Topics) for service selection
  - Frontend: Queue components (ManageQueues, CreateQueue, QueueDetails) with FIFO support and send/receive messages
  - Frontend: Topic components (ManageTopics, CreateTopic, TopicDetails, CreateSubscription) with publish messages and subscription management
  - Frontend: azure.js Vuex module with 13 actions for all Azure Service Bus operations
  - Frontend: Router integration with 2 lazy-loaded routes
  - Frontend: Azure navigation in sidebar with Microsoft Azure icon
  - Queue Features: Create/delete queues, send/receive messages, configurable properties (max size, delivery count, TTL, duplicate detection, sessions, dead-lettering)
  - Topic Features: Create/delete topics, publish messages, manage subscriptions, configurable properties (max size, TTL, duplicate detection, ordering)
  - Subscription Features: Create/delete subscriptions, configurable properties (max delivery count, lock duration, sessions, dead-lettering)
  - Activity tracking: Add/remove Azure servers, add/remove Service Bus queues, send queue messages, add/remove Service Bus topics, publish topic messages, add/remove subscriptions
  - Credential support: AES-256 encrypted Azure connection strings stored in ServerCredentials table
  - Region support: 30+ major Azure regions (East US, West US, West Europe, Southeast Asia, etc.)
  - Environment tagging: Development, Staging, Production support for Azure servers
  - Service type tracking: Differentiates between ServiceBus and future Azure services (Storage, EventHubs, etc.)

- **Multi-Environment Support**
  - Environment tagging for all servers (Kafka, Redis, RabbitMQ)
  - Three environment types: Development, Staging, Production
  - Environment selector in all Add/Edit server forms
  - Environment filter dropdown in all server list views (All, Development, Staging, Production)
  - Color-coded environment badges: Development (blue), Staging (orange), Production (red)
  - Environment breakdown in Dashboard cards showing server distribution
  - Backend: Database migration (003_AddEnvironmentColumn.sql) adds Environment column to all server tables
  - Backend: Updated entities (KafkaRecord, RedisRecord, RabbitMQRecord) with Environment property
  - Backend: Updated request DTOs with Environment parameter (defaults to Development)
  - Frontend: v-select components with environment dropdown in Add forms
  - Frontend: v-chip badges with color coding in list views and Dashboard
  - Backward compatibility: Default environment is 'Development' for existing servers

- **Dashboard with Server Metrics**
  - Centralized overview dashboard for all platforms (Kafka, Redis, RabbitMQ)
  - Real-time metrics display: server count, topic count, database count, queue count
  - Platform-specific cards with color-coded themes (Kafka: blue, Redis: red, RabbitMQ: orange)
  - Health status indicators showing Active/No Servers state
  - Quick actions section with shortcuts to add servers for each platform
  - Individual refresh buttons per platform for selective metric updates
  - Loading states with progress spinner during metric fetching
  - Error handling with retry functionality
  - Responsive grid layout using Vuetify components
  - **Dashboard is now the landing page** - loads immediately when app starts
  - Backward compatible redirect from /dashboard to /
  - Added missing RabbitMQ section to sidebar navigation

- **Production Readiness Improvements**
  - Pagination support with 100-item limit for Lists, Sets, Sorted Sets, and Streams
  - Loading states with progress spinners for all Redis management components
  - Error boundaries with inline error alerts and retry functionality
  - User-friendly error messages for failed operations
  - Pagination info alerts when data is limited to 100 items

- **Redis Advanced Data Types Support (Phases 1-5)**
  - **Lists** - Push/pop operations, view elements, manage list keys
  - **Hashes** - Set/get fields, view all fields, manage hash keys
  - **Sets** - Add/remove members, view set members, manage set keys
  - **Sorted Sets** - Add members with scores, view sorted members, manage sorted set keys
  - **Streams** - Add entries with multiple fields, view stream entries, manage stream keys
  - Backend: 25 new repository methods across all data types
  - Backend: 25 new FastEndpoints for full CRUD operations
  - Backend: 5 new request DTOs (PushListElementRequest, PopListElementRequest, SetHashFieldRequest, AddSetMemberRequest, AddSortedSetMemberRequest, AddStreamEntryRequest)
  - Frontend: ManageList.vue, ManageHash.vue, ManageSet.vue, ManageSortedSet.vue, ManageStream.vue components
  - Frontend: 25 new Vuex store actions for all data types
  - Frontend: Tabbed interface in ManageDatabases.vue with dedicated tabs for each data type
  - All operations support credential auto-detection for authenticated Redis servers

- **Error Handling UI (Phase 6)**
  - Centralized notification/toast system using Vuetify v-snackbar
  - Global notifications store module (notifications.js)
  - Toast notifications positioned at top-right corner
  - Success notifications (green, 4s auto-dismiss)
  - Error notifications (red, 6s auto-dismiss)
  - Warning notifications (orange, 5s auto-dismiss)
  - Enhanced logger utility with store integration and notification dispatch
  - Logger initialization in main.js with store instance
  - All Vuex store modules updated with success notifications (sqlite, kafka, redis, rabbitmq, credentials)
  - Removed redundant inline v-alert components from Vue components
  - Consistent error handling across all CRUD operations

- **Docker Test Environment**
  - docker-compose.test.yml with Kafka, RabbitMQ, Redis, Zookeeper
  - Kafka configured on port 9092 (PLAINTEXT, no authentication)
  - RabbitMQ with Management plugin on ports 5672/15672 (admin/admin123)
  - Redis with password authentication on port 6379 (redis123)
  - Kafka UI optional management interface on port 8090
  - Start scripts: start-test-env.ps1 (Windows) and start-test-env.sh (Linux/Mac)
  - TEST-CREDENTIALS.md comprehensive testing documentation
  - Health checks for all services
  - Persistent volumes for data retention

- **Activity Log Feature (Complete Implementation)**
  - Full activity tracking for all server and data operations
  - Backend: Database migration (004_CreateActivityLogTables.sql) creates activity_log and activity_type tables
  - Backend: 14 activity types seeded with Material Design colors and icons
  - Backend: Extended ActivityTypeEnum with 8 new values (Redis/RabbitMQ server operations, RabbitMQ exchange/queue operations)
  - Backend: Fixed IActivityLogRepository interface bug (added missing GetAllActivityLogs() method declaration)
  - Backend: Fixed ActivityType entity table mapping ([Table("activity_type")] attribute added)
  - Backend: Audit logging in KafkaRepository, RedisRepository, RabbitMqRepository for add/remove server operations
  - Backend: JSON details serialization (serverId, alias, environment) for all logged activities
  - Backend: 2 new FastEndpoints (GetActivityLogs, GetActivityTypes) with CORS support
  - Frontend: activityLog.js Vuex module with state management for logs and types
  - Frontend: ActivityLog.vue full-featured page component with table, filtering, and tooltips
  - Frontend: logsWithTypeData getter joins activity logs with type metadata (colors, icons)
  - Frontend: /activitylog route with lazy loading
  - Frontend: Functional "Activity Log" dropdown link in user navigation
  - UI: v-table with Timestamp, Activity Type (color-coded chips with icons), Description, and Details (JSON tooltip) columns
  - UI: Loading, error, and empty states with retry functionality
  - UI: Refresh button for manual log updates
  - UI: Formatted timestamps (date and time on separate lines)
  - Tracking: Add/remove Kafka servers
  - Tracking: Add/remove Redis servers
  - Tracking: Add/remove RabbitMQ servers
  - Tracking: Add/remove Kafka topics (existing functionality preserved)
  - Tracking: Add/remove Redis cache entries (existing functionality preserved)
  - Tracking: Add/remove RabbitMQ exchanges
  - Tracking: Add/remove RabbitMQ queues
  - Performance: Database indexes on timestamp (DESC) and activity_type_id for efficient queries

- **Dashboard Activity Widget**
  - Recent Activity widget displaying 5 most recent activity logs
  - Clickable card that navigates to full Activity Log page
  - Activity items with colored avatars, icons, and relative timestamps
  - Empty state when no activity exists
  - Auto-refreshes when Dashboard loads
  - Relative time formatting (Just now, X min ago, X hour(s) ago, X day(s) ago)

- **Activity Log Enhancements**
  - Filter by activity type dropdown with "All Types" option
  - Date range picker with From/To date fields and clear button
  - Search functionality for filtering log messages (case-insensitive)
  - Export to CSV with proper escaping and headers
  - Export to JSON with pretty-printed formatting
  - Filter summary bar showing active filters with individual clear buttons
  - Results counter showing "Showing X of Y activities"
  - Clear all filters button
  - "No results found" state when filters return empty results
  - Real-time reactive filtering across all filter types
  - Responsive layout with filters stacking on mobile
  - Export filename includes current date (activity-log-YYYY-MM-DD)

- **AWS SNS/SQS Support (Complete Implementation)**
  - Full integration with AWS Simple Notification Service (SNS) and Simple Queue Service (SQS)
  - Backend: New Nanuq.AWS class library project with modular structure
    - Nanuq.AWS/SQS/ - Complete SQS implementation
    - Nanuq.AWS/SNS/ - Complete SNS implementation
    - Nanuq.AWS/Common/ - Shared AWS credential helpers
  - Backend: AWS SDK v3.7.* packages (AWSSDK.SQS, AWSSDK.SimpleNotificationService, AWSSDK.Core)
  - Backend: SQS repository with 9 methods (GetQueuesAsync, GetQueueDetailsAsync, CreateQueueAsync, DeleteQueueAsync, SendMessageAsync, ReceiveMessagesAsync, DeleteMessageAsync, TestConnectionAsync)
  - Backend: SNS repository with 8 methods (GetTopicsAsync, GetTopicDetailsAsync, CreateTopicAsync, DeleteTopicAsync, PublishMessageAsync, GetSubscriptionsAsync, SubscribeAsync, UnsubscribeAsync)
  - Backend: 15 AWS operation endpoints (7 SQS + 8 SNS) with credential auto-detection
  - Backend: 5 SQLite CRUD endpoints for AWS server configurations (Add, Get, GetAll, Update, Delete)
  - Backend: Database migration (007_CreateAWSTables.sql) creates aws_servers table
  - Backend: Extended ActivityTypeEnum with 9 new AWS activity types (23-31)
  - Backend: AwsRepository with audit logging for add/remove server operations
  - Backend: Type aliases to resolve AWS SDK naming conflicts (SqsCreateQueueRequest, SnsPublishMessageRequest, etc.)
  - Backend: ServerCredentials integration for secure AWS access key storage
  - Frontend: Complete AWS management UI with 8 Vue components
  - Frontend: ListServers.vue with region, alias, environment display and add/delete functionality
  - Frontend: AddServer.vue with two-tab interface (Server Details | Credentials) and 15 AWS region dropdown
  - Frontend: ManageAWS.vue tabbed interface (SQS Queues | SNS Topics) for service selection
  - Frontend: SQS components (CreateQueue, QueueDetails) with FIFO support and dead-letter queue configuration
  - Frontend: SNS components (CreateTopic, TopicDetails, Subscribe) with subscription protocol validation
  - Frontend: aws.js Vuex module with 13 actions for all AWS operations
  - Frontend: Router integration with 5 lazy-loaded routes
  - Frontend: AWS navigation in sidebar with Material Design icon
  - SQS Features: Create/delete queues, send/receive messages, FIFO queue support, dead-letter queue configuration
  - SNS Features: Create/delete topics, publish messages, manage subscriptions, multiple protocol support (HTTP, HTTPS, Email, SMS, SQS, Lambda)
  - Activity tracking: Add/remove AWS servers, add/remove SQS queues, send SQS messages, add/remove SNS topics, publish SNS messages, add SNS subscriptions
  - Credential support: AES-256 encrypted AWS access keys stored in ServerCredentials table
  - Region support: All 15 major AWS regions (us-east-1, us-west-2, eu-west-1, ap-southeast-1, etc.)
  - Environment tagging: Development, Staging, Production support for AWS servers
  - Service type tracking: Differentiates between SQS, SNS, and future AWS services (S3, DynamoDB, etc.)

### Changed
- **Redis Data Fetching Performance**
  - Backend methods now use efficient Redis commands for pagination
  - Lists: `ListRangeAsync` with limit parameter
  - Sets: `SetScanAsync` for paginated iteration
  - Sorted Sets: `SortedSetRangeByRankWithScoresAsync` with limit
  - Streams: Standardized limit parameter naming
  - Prevents fetching massive datasets that could impact performance

### Fixed
- **Production Code Cleanup**
  - Removed all debug logging from production code
  - Cleaned up Console.WriteLine statements
  - Removed [CREDENTIAL-DEBUG] and [ENDPOINT-DEBUG] logging
  - Simplified DecryptIfNotNull to one-liner
  - Maintained only error logging for production diagnostics

- **Credential Decryption Error Handling**
  - Improved error handling for credential decryption failures
  - Graceful fallback when encryption key has changed (DPAPI key mismatch)
  - Clear error messages indicating when credentials need to be re-added
  - System continues to work without credentials when decryption fails
  - Enhanced logging for credential retrieval and decryption process

- **Credential Tab Issues**
  - Fixed credentials tab not enabling after saving Redis/Kafka/RabbitMQ servers
  - Store actions now return server ID directly from API response
  - Removed unreliable reload/search/match logic in AddServer components
  - Immediate tab enabling with proper state management

- **Modal Dialog Behavior**
  - Fixed modal not closing after saving credentials
  - Auto-close dialog 1.5 seconds after successful credential save
  - Hide "Save Server" button after server is saved
  - Proper server list reload with credential metadata after modal close

- **ManageDatabases.vue Null Reference**
  - Fixed "Cannot read properties of undefined (reading 'databaseCount')" error
  - Added null check for serverDetails before accessing properties
  - Added await this.$nextTick() for proper computed property updates

- **Optional Credentials Support**
  - Redis: Username optional, password required
  - Kafka: Both username and password optional (credentials only needed for SASL authentication)
  - RabbitMQ: Both username and password required
  - Info alerts in CredentialForm explaining optional credential requirements
  - Dynamic field labels and validation based on server type
  - Save button enabled/disabled logic based on server type requirements

- **RabbitMQ Management Features**
  - Complete exchange management (list, create, delete)
  - Complete queue management (list, create, delete, view details)
  - RabbitMQ Management HTTP API integration for listing operations
  - AMQP client integration for create/delete operations
  - Backend entities: Exchange, Queue, QueueDetails, Binding
  - Backend endpoints: 7 FastEndpoints (GetExchanges, AddExchange, DeleteExchange, GetQueues, GetQueueDetails, AddQueue, DeleteQueue)
  - Frontend components: ListServers, AddServer, ManageRabbitMQ (tabbed interface), AddExchange, AddQueue, QueueDetails
  - Vuex store module for RabbitMQ state management
  - Credential auto-detection for all RabbitMQ operations
  - RabbitMQ card visible on HomePage

- **Frontend Credential Management (Phase 1b)**
  - CredentialForm.vue reusable component with test connection
  - credentials.js Vuex module for state management
  - Tabbed interface in AddServer dialogs (Server Details / Credentials)
  - Authentication status indicators in server lists (shield icons)
  - Metadata preloading for credential status display

- **Auto-detect Credentials in Endpoints (Phase 5)**
  - All Kafka endpoints auto-detect credentials by serverId
  - All Redis endpoints auto-detect credentials by serverId
  - All RabbitMQ endpoints auto-detect credentials by serverId
  - Automatic LastUsedAt timestamp updates
  - Backward compatible (works without credentials)

- **Authentication & Security Infrastructure (Phase 1)**
  - Nanuq.Security project with AES-256 credential encryption
    - DPAPI-based key derivation for Windows
    - Random IV per encryption for enhanced security
    - Base64 encoding for database storage
  - Nanuq.Migrations project with DbUp for SQL-based migrations
    - Automatic migration execution on application startup
    - SchemaVersions tracking table
    - Embedded SQL migration scripts
  - ServerCredentials table for encrypted credential storage
    - Unique index on ServerId + ServerType
    - Automatic timestamp tracking (CreatedAt, UpdatedAt, LastUsedAt)
    - EncryptionKeyId for key rotation support
  - Credential management API endpoints (5 endpoints)
    - POST /credentials - Add encrypted credentials
    - GET /credentials/{serverId}/{serverType} - Get metadata (no password exposure)
    - PUT /credentials/{id} - Update credentials
    - DELETE /credentials/{id} - Delete credentials
    - POST /credentials/test - Test connection without saving
  - CredentialRepository with automatic encrypt/decrypt
  - KafkaConfigBuilder helper for SASL/PLAIN authentication
  - RedisConfigBuilder helper for password and ACL authentication

### Changed
- **RabbitMQ Migration (Phase 3)**
  - Removed plaintext Username and Password from RabbitMQRecord
  - Database migration to drop plaintext credential columns
  - RabbitMQ now uses encrypted ServerCredentials table
  - Created RabbitMQConfigBuilder helper for connection factory
  - Updated GetRabbitMQExchanges endpoint with credential auto-detection

- Nanuq.Sqlite project now references Nanuq.Security for credential encryption
- NanuqContext updated with ServerCredentials DbSet
- Program.cs updated to run migrations before DbContext initialization
- All Kafka repository methods accept optional ServerCredential parameter
- All Redis repository methods accept optional ServerCredential parameter

### Security
- All server credentials now encrypted at rest using AES-256
- Passwords never exposed in API responses (metadata only)
- Encryption service uses Windows DPAPI for secure key storage

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
