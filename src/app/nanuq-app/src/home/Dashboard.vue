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
              <v-col cols="12" md="4">
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
              <v-col cols="12" md="4">
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
              <v-col cols="12" md="4">
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
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import apiClient from '@/services/apiClient';

export default {
  name: 'Dashboard',
  data() {
    return {
      loading: false,
      error: null,
      refreshing: {
        kafka: false,
        redis: false,
        rabbitmq: false,
      },
      kafkaMetrics: {
        serverCount: 0,
        topicCount: 0,
      },
      redisMetrics: {
        serverCount: 0,
        databaseCount: 0,
      },
      rabbitMQMetrics: {
        serverCount: 0,
        queueCount: 0,
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

      this.kafkaMetrics.serverCount = kafkaServers.length;
      this.redisMetrics.serverCount = redisServers.length;
      this.rabbitMQMetrics.serverCount = rabbitMQServers.length;

      // Fetch detailed metrics in parallel
      await Promise.all([
        this.fetchKafkaTopicCount(kafkaServers),
        this.fetchRedisDatabaseCount(redisServers),
        this.fetchRabbitMQQueueCount(rabbitMQServers),
      ]);
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
