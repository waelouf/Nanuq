<template>
  <v-card elevation="2" class="activity-widget" @click="navigateToActivityLog">
    <v-card-title class="d-flex align-center justify-space-between">
      <div class="d-flex align-center">
        <v-icon class="mr-2">mdi-history</v-icon>
        Recent Activity
      </div>
      <v-btn icon size="small" variant="text">
        <v-icon>mdi-arrow-right</v-icon>
      </v-btn>
    </v-card-title>
    <v-divider />
    <v-card-text class="pa-0">
      <v-list v-if="recentLogs.length > 0" lines="two" density="compact">
        <v-list-item
          v-for="log in recentLogs"
          :key="log.id"
          class="activity-item"
        >
          <template #prepend>
            <v-avatar :color="getTypeColor(log)" size="32">
              <v-icon size="18" color="white">{{ getTypeIcon(log) }}</v-icon>
            </v-avatar>
          </template>
          <v-list-item-title>{{ log.log }}</v-list-item-title>
          <v-list-item-subtitle class="text-caption">
            {{ formatTimestamp(log.timestamp) }}
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>
      <div v-else class="pa-8 text-center text-medium-emphasis">
        <v-icon size="48" class="mb-2">mdi-history</v-icon>
        <div class="text-body-2">No recent activity</div>
      </div>
    </v-card-text>
  </v-card>
</template>

<script>
import { mapState, mapGetters } from 'vuex';

export default {
  name: 'ActivityLogWidget',
  computed: {
    ...mapState('activityLog', ['activityLogs', 'activityTypes']),
    ...mapGetters('activityLog', ['logsWithTypeData']),
    recentLogs() {
      return this.logsWithTypeData.slice(0, 5);
    },
  },
  async mounted() {
    await this.$store.dispatch('activityLog/refreshLogs');
  },
  methods: {
    getTypeColor(log) {
      return log.type?.color || 'grey';
    },
    getTypeIcon(log) {
      return log.type?.icon || 'mdi-information';
    },
    formatTimestamp(timestamp) {
      const now = new Date();
      const date = new Date(timestamp);
      const diffMs = now - date;
      const diffMins = Math.floor(diffMs / 60000);
      const diffHours = Math.floor(diffMs / 3600000);
      const diffDays = Math.floor(diffMs / 86400000);

      if (diffMins < 1) {
        return 'Just now';
      } else if (diffMins < 60) {
        return `${diffMins} min ago`;
      } else if (diffHours < 24) {
        return diffHours === 1 ? '1 hour ago' : `${diffHours} hours ago`;
      } else if (diffDays < 7) {
        return diffDays === 1 ? '1 day ago' : `${diffDays} days ago`;
      } else {
        return date.toLocaleDateString();
      }
    },
    navigateToActivityLog() {
      this.$router.push('/activitylog');
    },
  },
};
</script>

<style scoped>
.activity-widget {
  cursor: pointer;
  transition: transform 0.2s ease-in-out;
}

.activity-widget:hover {
  transform: translateY(-2px);
}

.activity-item {
  cursor: pointer;
}
</style>
