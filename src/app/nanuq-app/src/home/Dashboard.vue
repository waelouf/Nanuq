<template>
  <v-container fluid class="pa-6">
    <!-- Header -->
    <v-row class="mb-4">
      <v-col cols="12">
        <h1 class="text-h4 font-weight-bold">Dashboard</h1>
        <p class="text-subtitle-1 text-medium-emphasis">
          Overview of all connected servers and services
        </p>
      </v-col>
    </v-row>

    <!-- Loading State -->
    <v-row v-if="loading" class="justify-center">
      <v-col cols="12" class="text-center py-12">
        <v-progress-circular indeterminate size="64" color="primary" />
        <p class="text-subtitle-1 mt-4">Loading metrics...</p>
      </v-col>
    </v-row>

    <!-- Error State -->
    <v-alert
      v-if="error"
      type="error"
      variant="tonal"
      class="mb-4"
      closable
      @click:close="error = null"
    >
      {{ error }}
      <template #append>
        <v-btn color="error" variant="text" @click="refreshAllMetrics">Retry</v-btn>
      </template>
    </v-alert>

    <!-- Metrics Cards -->
    <v-row v-if="!loading">
      <!-- Kafka Card -->
      <v-col cols="12" md="4">
        <v-card class="h-100" elevation="2" hover>
          <v-card-title class="d-flex align-center bg-primary">
            <v-icon size="32" class="mr-3">mdi-apache-kafka</v-icon>
            <span class="text-h6">Kafka</span>
          </v-card-title>
          <v-card-text class="pa-6">
            <!-- Server Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="primary">mdi-server</v-icon>
              <div>
                <div class="text-h4 font-weight-bold">{{ kafkaMetrics.serverCount }}</div>
                <div class="text-caption text-medium-emphasis">Servers Configured</div>
              </div>
            </div>

            <!-- Topic Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="primary">mdi-message-text</v-icon>
              <div>
                <div class="text-h5">{{ kafkaMetrics.topicCount }}</div>
                <div class="text-caption text-medium-emphasis">Total Topics</div>
              </div>
            </div>

            <!-- Environment Breakdown -->
            <div v-if="kafkaMetrics.serverCount > 0" class="mb-4">
              <div class="text-caption text-medium-emphasis mb-2">Environments</div>
              <div class="d-flex gap-2 flex-wrap">
                <v-chip
                  v-if="kafkaMetrics.environments.Development > 0"
                  color="blue"
                  size="x-small"
                  variant="flat"
                >
                  Dev: {{ kafkaMetrics.environments.Development }}
                </v-chip>
                <v-chip
                  v-if="kafkaMetrics.environments.Staging > 0"
                  color="orange"
                  size="x-small"
                  variant="flat"
                >
                  Staging: {{ kafkaMetrics.environments.Staging }}
                </v-chip>
                <v-chip
                  v-if="kafkaMetrics.environments.Production > 0"
                  color="red"
                  size="x-small"
                  variant="flat"
                >
                  Prod: {{ kafkaMetrics.environments.Production }}
                </v-chip>
              </div>
            </div>

            <!-- Status Indicator -->
            <div class="d-flex align-center">
              <v-chip
                :color="kafkaMetrics.serverCount > 0 ? 'success' : 'grey'"
                size="small"
                variant="flat"
              >
                <v-icon start size="16">
                  {{ kafkaMetrics.serverCount > 0 ? 'mdi-check-circle' : 'mdi-minus-circle' }}
                </v-icon>
                {{ kafkaMetrics.serverCount > 0 ? 'Active' : 'No Servers' }}
              </v-chip>
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn
              color="primary"
              variant="text"
              :disabled="kafkaMetrics.serverCount === 0"
              @click="navigateTo('/kafka')"
            >
              <v-icon start>mdi-view-dashboard</v-icon>
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              icon
              size="small"
              variant="text"
              @click="refreshKafkaMetrics"
              :loading="refreshing.kafka"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <!-- Redis Card -->
      <v-col cols="12" md="4">
        <v-card class="h-100" elevation="2" hover>
          <v-card-title class="d-flex align-center bg-error">
            <v-icon size="32" class="mr-3">mdi-database</v-icon>
            <span class="text-h6">Redis</span>
          </v-card-title>
          <v-card-text class="pa-6">
            <!-- Server Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="error">mdi-server</v-icon>
              <div>
                <div class="text-h4 font-weight-bold">{{ redisMetrics.serverCount }}</div>
                <div class="text-caption text-medium-emphasis">Servers Configured</div>
              </div>
            </div>

            <!-- Database Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="error">mdi-database-outline</v-icon>
              <div>
                <div class="text-h5">{{ redisMetrics.databaseCount }}</div>
                <div class="text-caption text-medium-emphasis">Total Databases</div>
              </div>
            </div>

            <!-- Environment Breakdown -->
            <div v-if="redisMetrics.serverCount > 0" class="mb-4">
              <div class="text-caption text-medium-emphasis mb-2">Environments</div>
              <div class="d-flex gap-2 flex-wrap">
                <v-chip
                  v-if="redisMetrics.environments.Development > 0"
                  color="blue"
                  size="x-small"
                  variant="flat"
                >
                  Dev: {{ redisMetrics.environments.Development }}
                </v-chip>
                <v-chip
                  v-if="redisMetrics.environments.Staging > 0"
                  color="orange"
                  size="x-small"
                  variant="flat"
                >
                  Staging: {{ redisMetrics.environments.Staging }}
                </v-chip>
                <v-chip
                  v-if="redisMetrics.environments.Production > 0"
                  color="red"
                  size="x-small"
                  variant="flat"
                >
                  Prod: {{ redisMetrics.environments.Production }}
                </v-chip>
              </div>
            </div>

            <!-- Status Indicator -->
            <div class="d-flex align-center">
              <v-chip
                :color="redisMetrics.serverCount > 0 ? 'success' : 'grey'"
                size="small"
                variant="flat"
              >
                <v-icon start size="16">
                  {{ redisMetrics.serverCount > 0 ? 'mdi-check-circle' : 'mdi-minus-circle' }}
                </v-icon>
                {{ redisMetrics.serverCount > 0 ? 'Active' : 'No Servers' }}
              </v-chip>
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn
              color="error"
              variant="text"
              :disabled="redisMetrics.serverCount === 0"
              @click="navigateTo('/redis')"
            >
              <v-icon start>mdi-view-dashboard</v-icon>
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              icon
              size="small"
              variant="text"
              @click="refreshRedisMetrics"
              :loading="refreshing.redis"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <!-- RabbitMQ Card -->
      <v-col cols="12" md="4">
        <v-card class="h-100" elevation="2" hover>
          <v-card-title class="d-flex align-center bg-warning">
            <v-icon size="32" class="mr-3">mdi-rabbit</v-icon>
            <span class="text-h6">RabbitMQ</span>
          </v-card-title>
          <v-card-text class="pa-6">
            <!-- Server Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="warning">mdi-server</v-icon>
              <div>
                <div class="text-h4 font-weight-bold">{{ rabbitMQMetrics.serverCount }}</div>
                <div class="text-caption text-medium-emphasis">Servers Configured</div>
              </div>
            </div>

            <!-- Queue Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" color="warning">mdi-message-processing</v-icon>
              <div>
                <div class="text-h5">{{ rabbitMQMetrics.queueCount }}</div>
                <div class="text-caption text-medium-emphasis">Total Queues</div>
              </div>
            </div>

            <!-- Environment Breakdown -->
            <div v-if="rabbitMQMetrics.serverCount > 0" class="mb-4">
              <div class="text-caption text-medium-emphasis mb-2">Environments</div>
              <div class="d-flex gap-2 flex-wrap">
                <v-chip
                  v-if="rabbitMQMetrics.environments.Development > 0"
                  color="blue"
                  size="x-small"
                  variant="flat"
                >
                  Dev: {{ rabbitMQMetrics.environments.Development }}
                </v-chip>
                <v-chip
                  v-if="rabbitMQMetrics.environments.Staging > 0"
                  color="orange"
                  size="x-small"
                  variant="flat"
                >
                  Staging: {{ rabbitMQMetrics.environments.Staging }}
                </v-chip>
                <v-chip
                  v-if="rabbitMQMetrics.environments.Production > 0"
                  color="red"
                  size="x-small"
                  variant="flat"
                >
                  Prod: {{ rabbitMQMetrics.environments.Production }}
                </v-chip>
              </div>
            </div>

            <!-- Status Indicator -->
            <div class="d-flex align-center">
              <v-chip
                :color="rabbitMQMetrics.serverCount > 0 ? 'success' : 'grey'"
                size="small"
                variant="flat"
              >
                <v-icon start size="16">
                  {{ rabbitMQMetrics.serverCount > 0 ? 'mdi-check-circle' : 'mdi-minus-circle' }}
                </v-icon>
                {{ rabbitMQMetrics.serverCount > 0 ? 'Active' : 'No Servers' }}
              </v-chip>
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn
              color="warning"
              variant="text"
              :disabled="rabbitMQMetrics.serverCount === 0"
              @click="navigateTo('/rabbitmq')"
            >
              <v-icon start>mdi-view-dashboard</v-icon>
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              icon
              size="small"
              variant="text"
              @click="refreshRabbitMQMetrics"
              :loading="refreshing.rabbitmq"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <!-- AWS Card -->
      <v-col cols="12" md="3">
        <v-card class="h-100" elevation="2" hover>
          <v-card-title class="d-flex align-center" style="background-color: #FF9900;">
            <v-icon size="32" class="mr-3">mdi-aws</v-icon>
            <span class="text-h6">AWS</span>
          </v-card-title>
          <v-card-text class="pa-6">
            <!-- Server Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" style="color: #FF9900;">mdi-server</v-icon>
              <div>
                <div class="text-h4 font-weight-bold">{{ awsMetrics.serverCount }}</div>
                <div class="text-caption text-medium-emphasis">Servers Configured</div>
              </div>
            </div>

            <!-- Resource Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" style="color: #FF9900;">mdi-cloud-outline</v-icon>
              <div>
                <div class="text-h5">{{ awsMetrics.resourceCount }}</div>
                <div class="text-caption text-medium-emphasis">SQS + SNS Resources</div>
              </div>
            </div>

            <!-- Environment Breakdown -->
            <div v-if="awsMetrics.serverCount > 0" class="mb-4">
              <div class="text-caption text-medium-emphasis mb-2">Environments</div>
              <div class="d-flex gap-2 flex-wrap">
                <v-chip
                  v-if="awsMetrics.environments.Development > 0"
                  color="blue"
                  size="x-small"
                  variant="flat"
                >
                  Dev: {{ awsMetrics.environments.Development }}
                </v-chip>
                <v-chip
                  v-if="awsMetrics.environments.Staging > 0"
                  color="orange"
                  size="x-small"
                  variant="flat"
                >
                  Staging: {{ awsMetrics.environments.Staging }}
                </v-chip>
                <v-chip
                  v-if="awsMetrics.environments.Production > 0"
                  color="red"
                  size="x-small"
                  variant="flat"
                >
                  Prod: {{ awsMetrics.environments.Production }}
                </v-chip>
              </div>
            </div>

            <!-- Status Indicator -->
            <div class="d-flex align-center">
              <v-chip
                :color="awsMetrics.serverCount > 0 ? 'success' : 'grey'"
                size="small"
                variant="flat"
              >
                <v-icon start size="16">
                  {{ awsMetrics.serverCount > 0 ? 'mdi-check-circle' : 'mdi-minus-circle' }}
                </v-icon>
                {{ awsMetrics.serverCount > 0 ? 'Active' : 'No Servers' }}
              </v-chip>
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn
              style="color: #FF9900;"
              variant="text"
              :disabled="awsMetrics.serverCount === 0"
              @click="navigateTo('/aws')"
            >
              <v-icon start>mdi-view-dashboard</v-icon>
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              icon
              size="small"
              variant="text"
              @click="refreshAWSMetrics"
              :loading="refreshing.aws"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <!-- Azure Card -->
      <v-col cols="12" md="3">
        <v-card class="h-100" elevation="2" hover>
          <v-card-title class="d-flex align-center" style="background-color: #0078D4; color: white;">
            <v-icon size="32" class="mr-3" color="white">mdi-microsoft-azure</v-icon>
            <span class="text-h6">Azure</span>
          </v-card-title>
          <v-card-text class="pa-6">
            <!-- Server Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" style="color: #0078D4;">mdi-server</v-icon>
              <div>
                <div class="text-h4 font-weight-bold">{{ azureMetrics.serverCount }}</div>
                <div class="text-caption text-medium-emphasis">Servers Configured</div>
              </div>
            </div>

            <!-- Resource Count -->
            <div class="d-flex align-center mb-4">
              <v-icon size="24" class="mr-3" style="color: #0078D4;">mdi-cloud-outline</v-icon>
              <div>
                <div class="text-h5">{{ azureMetrics.resourceCount }}</div>
                <div class="text-caption text-medium-emphasis">Queues + Topics</div>
              </div>
            </div>

            <!-- Environment Breakdown -->
            <div v-if="azureMetrics.serverCount > 0" class="mb-4">
              <div class="text-caption text-medium-emphasis mb-2">Environments</div>
              <div class="d-flex gap-2 flex-wrap">
                <v-chip
                  v-if="azureMetrics.environments.Development > 0"
                  color="blue"
                  size="x-small"
                  variant="flat"
                >
                  Dev: {{ azureMetrics.environments.Development }}
                </v-chip>
                <v-chip
                  v-if="azureMetrics.environments.Staging > 0"
                  color="orange"
                  size="x-small"
                  variant="flat"
                >
                  Staging: {{ azureMetrics.environments.Staging }}
                </v-chip>
                <v-chip
                  v-if="azureMetrics.environments.Production > 0"
                  color="red"
                  size="x-small"
                  variant="flat"
                >
                  Prod: {{ azureMetrics.environments.Production }}
                </v-chip>
              </div>
            </div>

            <!-- Status Indicator -->
            <div class="d-flex align-center">
              <v-chip
                :color="azureMetrics.serverCount > 0 ? 'success' : 'grey'"
                size="small"
                variant="flat"
              >
                <v-icon start size="16">
                  {{ azureMetrics.serverCount > 0 ? 'mdi-check-circle' : 'mdi-minus-circle' }}
                </v-icon>
                {{ azureMetrics.serverCount > 0 ? 'Active' : 'No Servers' }}
              </v-chip>
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn
              style="color: #0078D4;"
              variant="text"
              :disabled="azureMetrics.serverCount === 0"
              @click="navigateTo('/azure')"
            >
              <v-icon start>mdi-view-dashboard</v-icon>
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              icon
              size="small"
              variant="text"
              @click="refreshAzureMetrics"
              :loading="refreshing.azure"
            >
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Quick Actions -->
    <v-row v-if="!loading" class="mt-4">
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="d-flex align-center">
            <v-icon class="mr-2">mdi-lightning-bolt</v-icon>
            Quick Actions
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col cols="12" md="2.4">
                <v-btn
                  block
                  color="primary"
                  variant="outlined"
                  size="large"
                  @click="navigateTo('/kafka/add')"
                >
                  <v-icon start>mdi-plus</v-icon>
                  Add Kafka Server
                </v-btn>
              </v-col>
              <v-col cols="12" md="2.4">
                <v-btn
                  block
                  color="error"
                  variant="outlined"
                  size="large"
                  @click="navigateTo('/redis')"
                >
                  <v-icon start>mdi-plus</v-icon>
                  Add Redis Server
                </v-btn>
              </v-col>
              <v-col cols="12" md="2.4">
                <v-btn
                  block
                  color="warning"
                  variant="outlined"
                  size="large"
                  @click="navigateTo('/rabbitmq')"
                >
                  <v-icon start>mdi-plus</v-icon>
                  Add RabbitMQ Server
                </v-btn>
              </v-col>
              <v-col cols="12" md="2.4">
                <v-btn
                  block
                  style="color: #FF9900; border-color: #FF9900;"
                  variant="outlined"
                  size="large"
                  @click="navigateTo('/aws')"
                >
                  <v-icon start>mdi-plus</v-icon>
                  Add AWS Server
                </v-btn>
              </v-col>
              <v-col cols="12" md="2.4">
                <v-btn
                  block
                  style="color: #0078D4; border-color: #0078D4;"
                  variant="outlined"
                  size="large"
                  @click="navigateTo('/azure')"
                >
                  <v-icon start>mdi-plus</v-icon>
                  Add Azure Server
                </v-btn>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Recent Activity Widget -->
    <v-row v-if="!loading" class="mt-4">
      <v-col cols="12">
        <ActivityLogWidget />
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import apiClient from '@/services/apiClient';
import ActivityLogWidget from '@/components/ActivityLogWidget.vue';

export default {
  name: 'Dashboard',
  components: {
    ActivityLogWidget,
  },
  data() {
    return {
      loading: false,
      error: null,
      refreshing: {
        kafka: false,
        redis: false,
        rabbitmq: false,
        aws: false,
        azure: false,
      },
      kafkaMetrics: {
        serverCount: 0,
        topicCount: 0,
        environments: {
          Development: 0,
          Staging: 0,
          Production: 0,
        },
      },
      redisMetrics: {
        serverCount: 0,
        databaseCount: 0,
        environments: {
          Development: 0,
          Staging: 0,
          Production: 0,
        },
      },
      rabbitMQMetrics: {
        serverCount: 0,
        queueCount: 0,
        environments: {
          Development: 0,
          Staging: 0,
          Production: 0,
        },
      },
      awsMetrics: {
        serverCount: 0,
        resourceCount: 0,
        environments: {
          Development: 0,
          Staging: 0,
          Production: 0,
        },
      },
      azureMetrics: {
        serverCount: 0,
        resourceCount: 0,
        environments: {
          Development: 0,
          Staging: 0,
          Production: 0,
        },
      },
    };
  },
  async mounted() {
    await this.loadAllMetrics();
  },
  methods: {
    async loadAllMetrics() {
      try {
        this.loading = true;
        this.error = null;

        // Load all server lists from store
        await Promise.all([
          this.$store.dispatch('sqlite/loadKafkaServers'),
          this.$store.dispatch('sqlite/loadRedisServers'),
          this.$store.dispatch('sqlite/loadRabbitMQServers'),
          this.$store.dispatch('sqlite/loadAwsServers'),
          this.$store.dispatch('sqlite/loadAzureServers'),
        ]);

        // Update metrics from store state and fetch detailed metrics
        await this.updateMetrics();
      } catch (error) {
        this.error = 'Failed to load dashboard metrics. Please try again.';
        console.error('Error loading dashboard metrics:', error);
      } finally {
        this.loading = false;
      }
    },
    async updateMetrics() {
      // Get server counts from store
      const kafkaServers = this.$store.state.sqlite.kafkaServers || [];
      const redisServers = this.$store.state.sqlite.redisServers || [];
      const rabbitMQServers = this.$store.state.sqlite.rabbitMQServers || [];
      const awsServers = this.$store.state.sqlite.awsServers || [];
      const azureServers = this.$store.state.sqlite.azureServers || [];

      this.kafkaMetrics.serverCount = kafkaServers.length;
      this.redisMetrics.serverCount = redisServers.length;
      this.rabbitMQMetrics.serverCount = rabbitMQServers.length;
      this.awsMetrics.serverCount = awsServers.length;
      this.azureMetrics.serverCount = azureServers.length;

      // Calculate environment counts
      this.kafkaMetrics.environments = this.countEnvironments(kafkaServers);
      this.redisMetrics.environments = this.countEnvironments(redisServers);
      this.rabbitMQMetrics.environments = this.countEnvironments(rabbitMQServers);
      this.awsMetrics.environments = this.countEnvironments(awsServers);
      this.azureMetrics.environments = this.countEnvironments(azureServers);

      // Fetch detailed metrics in parallel
      await Promise.all([
        this.fetchKafkaTopicCount(kafkaServers),
        this.fetchRedisDatabaseCount(redisServers),
        this.fetchRabbitMQQueueCount(rabbitMQServers),
        this.fetchAWSResourceCount(awsServers),
        this.fetchAzureResourceCount(azureServers),
      ]);
    },
    countEnvironments(servers) {
      const counts = {
        Development: 0,
        Staging: 0,
        Production: 0,
      };
      servers.forEach((server) => {
        const env = server.environment || 'Development';
        if (counts.hasOwnProperty(env)) {
          counts[env]++;
        }
      });
      return counts;
    },
    async fetchKafkaTopicCount(kafkaServers) {
      try {
        let totalTopics = 0;
        // Fetch topics for each Kafka server
        for (const server of kafkaServers) {
          try {
            const response = await apiClient.get(`/kafka/topic/${server.bootstrapServer}`);
            if (response.data && Array.isArray(response.data)) {
              totalTopics += response.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch topics for Kafka server ${server.bootstrapServer}:`, error);
          }
        }
        this.kafkaMetrics.topicCount = totalTopics;
      } catch (error) {
        console.error('Error fetching Kafka topic count:', error);
        this.kafkaMetrics.topicCount = 0;
      }
    },
    async fetchRedisDatabaseCount(redisServers) {
      try {
        let totalDatabases = 0;
        // Fetch database count for each Redis server
        for (const server of redisServers) {
          try {
            const response = await apiClient.get(`/redis/server/${server.serverUrl}`);
            if (response.data && response.data.databaseCount) {
              totalDatabases += response.data.databaseCount;
            }
          } catch (error) {
            console.warn(`Failed to fetch databases for Redis server ${server.serverUrl}:`, error);
          }
        }
        this.redisMetrics.databaseCount = totalDatabases;
      } catch (error) {
        console.error('Error fetching Redis database count:', error);
        this.redisMetrics.databaseCount = 0;
      }
    },
    async fetchRabbitMQQueueCount(rabbitMQServers) {
      try {
        let totalQueues = 0;
        // Fetch queues for each RabbitMQ server
        for (const server of rabbitMQServers) {
          try {
            const response = await apiClient.get(`/rabbitmq/queues/${server.serverUrl}`);
            if (response.data && Array.isArray(response.data)) {
              totalQueues += response.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch queues for RabbitMQ server ${server.serverUrl}:`, error);
          }
        }
        this.rabbitMQMetrics.queueCount = totalQueues;
      } catch (error) {
        console.error('Error fetching RabbitMQ queue count:', error);
        this.rabbitMQMetrics.queueCount = 0;
      }
    },
    async refreshAllMetrics() {
      await this.loadAllMetrics();
    },
    async refreshKafkaMetrics() {
      try {
        this.refreshing.kafka = true;
        await this.$store.dispatch('sqlite/loadKafkaServers');
        this.updateMetrics();
      } catch (error) {
        console.error('Error refreshing Kafka metrics:', error);
      } finally {
        this.refreshing.kafka = false;
      }
    },
    async refreshRedisMetrics() {
      try {
        this.refreshing.redis = true;
        await this.$store.dispatch('sqlite/loadRedisServers');
        this.updateMetrics();
      } catch (error) {
        console.error('Error refreshing Redis metrics:', error);
      } finally {
        this.refreshing.redis = false;
      }
    },
    async refreshRabbitMQMetrics() {
      try {
        this.refreshing.rabbitmq = true;
        await this.$store.dispatch('sqlite/loadRabbitMQServers');
        this.updateMetrics();
      } catch (error) {
        console.error('Error refreshing RabbitMQ metrics:', error);
      } finally {
        this.refreshing.rabbitmq = false;
      }
    },
    async fetchAWSResourceCount(awsServers) {
      try {
        let totalResources = 0;
        // Fetch SQS queues and SNS topics for each AWS server
        for (const server of awsServers) {
          try {
            // Fetch SQS queues
            const sqsResponse = await apiClient.get(`/aws/sqs/queues/${server.id}`);
            if (sqsResponse.data && Array.isArray(sqsResponse.data)) {
              totalResources += sqsResponse.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch SQS queues for AWS server ${server.id}:`, error);
          }

          try {
            // Fetch SNS topics
            const snsResponse = await apiClient.get(`/aws/sns/topics/${server.id}`);
            if (snsResponse.data && Array.isArray(snsResponse.data)) {
              totalResources += snsResponse.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch SNS topics for AWS server ${server.id}:`, error);
          }
        }
        this.awsMetrics.resourceCount = totalResources;
      } catch (error) {
        console.error('Error fetching AWS resource count:', error);
        this.awsMetrics.resourceCount = 0;
      }
    },
    async refreshAWSMetrics() {
      try {
        this.refreshing.aws = true;
        await this.$store.dispatch('sqlite/loadAwsServers');
        this.updateMetrics();
      } catch (error) {
        console.error('Error refreshing AWS metrics:', error);
      } finally {
        this.refreshing.aws = false;
      }
    },
    async fetchAzureResourceCount(azureServers) {
      try {
        let totalResources = 0;
        // Fetch queues and topics for each Azure server
        for (const server of azureServers) {
          try {
            // Fetch queues
            const queuesResponse = await apiClient.get(`/azure/servicebus/queues/${server.id}`);
            if (queuesResponse.data && Array.isArray(queuesResponse.data)) {
              totalResources += queuesResponse.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch queues for Azure server ${server.id}:`, error);
          }

          try {
            // Fetch topics
            const topicsResponse = await apiClient.get(`/azure/servicebus/topics/${server.id}`);
            if (topicsResponse.data && Array.isArray(topicsResponse.data)) {
              totalResources += topicsResponse.data.length;
            }
          } catch (error) {
            console.warn(`Failed to fetch topics for Azure server ${server.id}:`, error);
          }
        }
        this.azureMetrics.resourceCount = totalResources;
      } catch (error) {
        console.error('Error fetching Azure resource count:', error);
        this.azureMetrics.resourceCount = 0;
      }
    },
    async refreshAzureMetrics() {
      try {
        this.refreshing.azure = true;
        await this.$store.dispatch('sqlite/loadAzureServers');
        this.updateMetrics();
      } catch (error) {
        console.error('Error refreshing Azure metrics:', error);
      } finally {
        this.refreshing.azure = false;
      }
    },
    navigateTo(path) {
      this.$router.push(path);
    },
  },
};
</script>

<style scoped>
.h-100 {
  height: 100%;
}
</style>
