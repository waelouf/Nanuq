<template>
  <v-container fluid class="pa-6">
    <!-- Header -->
    <v-row class="mb-4">
      <v-col cols="12">
        <div>
          <h1 class="text-h4 font-weight-bold">Activity Log</h1>
          <p class="text-subtitle-1 text-medium-emphasis">
            Track all operations across all servers
          </p>
        </div>
      </v-col>
    </v-row>

    <!-- Filters -->
    <v-row class="mb-4">
      <!-- Activity Type Filter -->
      <v-col cols="12" md="3">
        <v-select
          v-model="selectedActivityType"
          :items="activityTypeOptions"
          label="Activity Type"
          variant="outlined"
          density="compact"
          hide-details
        />
      </v-col>

      <!-- Date Range Filter -->
      <v-col cols="12" md="3">
        <div class="d-flex align-center ga-2">
          <v-text-field
            v-model="fromDate"
            type="date"
            label="From Date"
            variant="outlined"
            density="compact"
            hide-details
            style="flex: 1;"
          />
          <v-text-field
            v-model="toDate"
            type="date"
            label="To Date"
            variant="outlined"
            density="compact"
            hide-details
            style="flex: 1;"
          />
          <v-btn
            icon="mdi-close"
            size="small"
            variant="text"
            @click="clearDateFilters"
            :disabled="!fromDate && !toDate"
          />
        </div>
      </v-col>

      <!-- Search Field -->
      <v-col cols="12" md="4">
        <v-text-field
          v-model="searchQuery"
          label="Search messages"
          variant="outlined"
          density="compact"
          hide-details
          clearable
          prepend-inner-icon="mdi-magnify"
        />
      </v-col>

      <!-- Action Buttons -->
      <v-col cols="12" md="2" class="d-flex justify-end ga-2">
        <v-btn
          color="primary"
          variant="elevated"
          prepend-icon="mdi-refresh"
          @click="handleRefresh"
          :loading="loading"
        >
          Refresh
        </v-btn>

        <!-- Export Menu -->
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn
              v-bind="props"
              color="primary"
              variant="elevated"
              icon="mdi-download"
            />
          </template>
          <v-list>
            <v-list-item @click="exportToCSV">
              <v-list-item-title>
                <v-icon size="small" class="mr-2">mdi-file-delimited</v-icon>
                Export to CSV
              </v-list-item-title>
            </v-list-item>
            <v-list-item @click="exportToJSON">
              <v-list-item-title>
                <v-icon size="small" class="mr-2">mdi-code-json</v-icon>
                Export to JSON
              </v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </v-col>
    </v-row>

    <!-- Filter Summary Bar -->
    <v-row v-if="hasActiveFilters" class="mb-4">
      <v-col cols="12">
        <v-card variant="tonal" color="info">
          <v-card-text class="py-2">
            <div class="d-flex align-center flex-wrap ga-2">
              <span class="text-subtitle-2 mr-2">Active Filters:</span>
              <v-chip
                v-for="filter in filterSummary"
                :key="filter.type"
                size="small"
                closable
                @click:close="clearFilter(filter.type)"
              >
                {{ filter.label }}
              </v-chip>
              <v-spacer />
              <span class="text-subtitle-2">
                Showing {{ filteredLogs.length }} of {{ logsWithTypeData.length }} activities
              </span>
              <v-btn
                size="small"
                variant="text"
                @click="clearAllFilters"
              >
                Clear All
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
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

    <!-- No Results Found (Filters Active) -->
    <v-row v-if="!loading && logsWithTypeData.length > 0 && filteredLogs.length === 0" class="justify-center">
      <v-col cols="12" md="6" class="text-center py-12">
        <v-icon size="96" color="grey-lighten-1" class="mb-4">mdi-filter-remove</v-icon>
        <h2 class="text-h5 mb-2">No Results Found</h2>
        <p class="text-body-1 text-medium-emphasis">
          No activity logs match your current filters. Try adjusting your search criteria.
        </p>
        <v-btn
          color="primary"
          variant="outlined"
          class="mt-4"
          @click="clearAllFilters"
        >
          Clear All Filters
        </v-btn>
      </v-col>
    </v-row>

    <!-- Activity Log Table -->
    <v-row v-if="!loading && filteredLogs.length > 0">
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
                <tr v-for="log in filteredLogs" :key="log.id">
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
  data() {
    return {
      selectedActivityType: 'All Types',
      fromDate: null,
      toDate: null,
      searchQuery: '',
    };
  },
  computed: {
    ...mapState('activityLog', ['activityLogs', 'activityTypes', 'loading', 'error']),
    ...mapGetters('activityLog', ['logsWithTypeData']),

    /**
     * Activity type options for dropdown (includes "All Types")
     */
    activityTypeOptions() {
      return ['All Types', ...this.activityTypes.map(t => t.name)];
    },

    /**
     * Filtered logs based on activity type, date range, and search query
     */
    filteredLogs() {
      let logs = this.logsWithTypeData;

      // Filter by activity type
      if (this.selectedActivityType !== 'All Types') {
        logs = logs.filter(log => log.type?.name === this.selectedActivityType);
      }

      // Filter by date range (from date)
      if (this.fromDate) {
        logs = logs.filter(log => new Date(log.timestamp) >= new Date(this.fromDate));
      }

      // Filter by date range (to date - end of day)
      if (this.toDate) {
        const toDateEnd = new Date(this.toDate);
        toDateEnd.setHours(23, 59, 59, 999);
        logs = logs.filter(log => new Date(log.timestamp) <= toDateEnd);
      }

      // Filter by search query
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        logs = logs.filter(log => log.log.toLowerCase().includes(query));
      }

      return logs;
    },

    /**
     * Check if any filters are active
     */
    hasActiveFilters() {
      return this.selectedActivityType !== 'All Types' ||
             this.fromDate ||
             this.toDate ||
             this.searchQuery;
    },

    /**
     * Summary of active filters for display
     */
    filterSummary() {
      const summary = [];
      if (this.selectedActivityType !== 'All Types') {
        summary.push({ type: 'activityType', label: `Type: ${this.selectedActivityType}` });
      }
      if (this.fromDate) {
        summary.push({ type: 'fromDate', label: `After: ${this.fromDate}` });
      }
      if (this.toDate) {
        summary.push({ type: 'toDate', label: `Before: ${this.toDate}` });
      }
      if (this.searchQuery) {
        summary.push({ type: 'search', label: `Search: "${this.searchQuery}"` });
      }
      return summary;
    },
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

    /**
     * Clear date filters
     */
    clearDateFilters() {
      this.fromDate = null;
      this.toDate = null;
    },

    /**
     * Clear a specific filter
     */
    clearFilter(filterType) {
      if (filterType === 'activityType') this.selectedActivityType = 'All Types';
      if (filterType === 'fromDate') this.fromDate = null;
      if (filterType === 'toDate') this.toDate = null;
      if (filterType === 'search') this.searchQuery = '';
    },

    /**
     * Clear all filters
     */
    clearAllFilters() {
      this.selectedActivityType = 'All Types';
      this.fromDate = null;
      this.toDate = null;
      this.searchQuery = '';
    },

    /**
     * Export filtered logs to CSV format
     */
    exportToCSV() {
      const headers = ['Timestamp', 'Activity Type', 'Description', 'Details'];
      const rows = this.filteredLogs.map(log => [
        this.formatDate(log.timestamp) + ' ' + this.formatTime(log.timestamp),
        log.type?.name || 'Unknown',
        log.log,
        log.details || '',
      ]);

      const csvContent = [
        headers.join(','),
        ...rows.map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(',')),
      ].join('\n');

      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      const date = new Date().toISOString().split('T')[0];
      link.setAttribute('href', url);
      link.setAttribute('download', `activity-log-${date}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    },

    /**
     * Export filtered logs to JSON format
     */
    exportToJSON() {
      const jsonContent = JSON.stringify(this.filteredLogs, null, 2);
      const blob = new Blob([jsonContent], { type: 'application/json' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      const date = new Date().toISOString().split('T')[0];
      link.setAttribute('href', url);
      link.setAttribute('download', `activity-log-${date}.json`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
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
