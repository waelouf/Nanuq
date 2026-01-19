<template>
  <v-container fluid class="pa-6">
    <!-- Header -->
    <v-row class="mb-4">
      <v-col cols="12">
        <div class="d-flex align-center justify-space-between">
          <div>
            <h1 class="text-h4 font-weight-bold">Activity Log</h1>
            <p class="text-subtitle-1 text-medium-emphasis">
              Track all operations across Kafka, Redis, and RabbitMQ
            </p>
          </div>
          <v-btn
            color="primary"
            variant="elevated"
            prepend-icon="mdi-refresh"
            @click="handleRefresh"
            :loading="loading"
          >
            Refresh
          </v-btn>
        </div>
      </v-col>
    </v-row>

    <!-- Loading State -->
    <v-row v-if="loading && activityLogs.length === 0" class="justify-center">
      <v-col cols="12" class="text-center py-12">
        <v-progress-circular indeterminate size="64" color="primary" />
        <p class="text-subtitle-1 mt-4">Loading activity logs...</p>
      </v-col>
    </v-row>

    <!-- Error State -->
    <v-alert
      v-if="error"
      type="error"
      variant="tonal"
      class="mb-4"
      closable
      @click:close="clearError"
    >
      {{ error }}
      <template #append>
        <v-btn color="error" variant="text" @click="handleRefresh">Retry</v-btn>
      </template>
    </v-alert>

    <!-- Empty State -->
    <v-row v-if="!loading && logsWithTypeData.length === 0 && !error" class="justify-center">
      <v-col cols="12" md="6" class="text-center py-12">
        <v-icon size="96" color="grey-lighten-1" class="mb-4">mdi-history</v-icon>
        <h2 class="text-h5 mb-2">No Activity Yet</h2>
        <p class="text-body-1 text-medium-emphasis">
          Activity logs will appear here when you add or remove servers, topics, queues, or cache entries.
        </p>
      </v-col>
    </v-row>

    <!-- Activity Log Table -->
    <v-row v-if="!loading && logsWithTypeData.length > 0">
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-text class="pa-0">
            <v-table fixed-header height="600px">
              <thead>
                <tr>
                  <th class="text-left" style="width: 180px;">Timestamp</th>
                  <th class="text-left" style="width: 200px;">Activity Type</th>
                  <th class="text-left">Description</th>
                  <th class="text-center" style="width: 80px;">Details</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="log in logsWithTypeData" :key="log.id">
                  <!-- Timestamp -->
                  <td class="text-no-wrap">
                    <div class="text-body-2 font-weight-medium">{{ formatDate(log.timestamp) }}</div>
                    <div class="text-caption text-medium-emphasis">{{ formatTime(log.timestamp) }}</div>
                  </td>

                  <!-- Activity Type -->
                  <td>
                    <v-chip
                      v-if="log.type"
                      :color="log.type.color"
                      :prepend-icon="log.type.icon"
                      size="small"
                      variant="flat"
                      class="font-weight-medium"
                    >
                      {{ log.type.name }}
                    </v-chip>
                    <span v-else class="text-caption text-medium-emphasis">Unknown</span>
                  </td>

                  <!-- Description -->
                  <td>
                    <div class="text-body-2">{{ log.log }}</div>
                  </td>

                  <!-- Details Tooltip -->
                  <td class="text-center">
                    <v-tooltip
                      v-if="log.details"
                      location="left"
                      max-width="400"
                    >
                      <template v-slot:activator="{ props }">
                        <v-btn
                          v-bind="props"
                          icon="mdi-information-outline"
                          size="small"
                          variant="text"
                          color="grey"
                        />
                      </template>
                      <div class="pa-2">
                        <div class="text-subtitle-2 mb-2">Details:</div>
                        <pre class="text-caption">{{ formatDetails(log.details) }}</pre>
                      </div>
                    </v-tooltip>
                    <span v-else class="text-caption text-medium-emphasis">-</span>
                  </td>
                </tr>
              </tbody>
            </v-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import { mapState, mapGetters, mapActions, mapMutations } from 'vuex';

export default {
  name: 'ActivityLog',
  computed: {
    ...mapState('activityLog', ['activityLogs', 'activityTypes', 'loading', 'error']),
    ...mapGetters('activityLog', ['logsWithTypeData']),
  },
  methods: {
    ...mapActions('activityLog', ['loadActivityLogs', 'loadActivityTypes', 'refreshLogs']),
    ...mapMutations('activityLog', ['clearError']),

    /**
     * Format timestamp to date string (e.g., "Jan 18, 2026")
     */
    formatDate(timestamp) {
      if (!timestamp) return '-';
      const date = new Date(timestamp);
      return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      });
    },

    /**
     * Format timestamp to time string (e.g., "14:35:22")
     */
    formatTime(timestamp) {
      if (!timestamp) return '-';
      const date = new Date(timestamp);
      return date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      });
    },

    /**
     * Format JSON details for tooltip display
     */
    formatDetails(details) {
      if (!details) return '-';
      try {
        const parsed = typeof details === 'string' ? JSON.parse(details) : details;
        return JSON.stringify(parsed, null, 2);
      } catch (error) {
        return details;
      }
    },

    /**
     * Handle refresh button click
     */
    async handleRefresh() {
      try {
        await this.refreshLogs();
      } catch (error) {
        // Error handling is done in the store action
      }
    },
  },
  async mounted() {
    // Load activity logs and types on component mount
    try {
      await this.refreshLogs();
    } catch (error) {
      // Error handling is done in the store action
    }
  },
};
</script>

<style scoped>
/* Ensure pre tag in tooltip doesn't overflow */
pre {
  white-space: pre-wrap;
  word-wrap: break-word;
  font-family: 'Courier New', monospace;
  margin: 0;
}

/* Make table rows hoverable */
tbody tr:hover {
  background-color: rgba(0, 0, 0, 0.04);
}
</style>
