<template>
  <v-card-title>
    <span class="text-h5">Queue Details: {{ queueName }}</span>
  </v-card-title>
  <v-card-text>
    <div v-if="details">
      <v-list>
        <v-list-item>
          <v-list-item-title>Name</v-list-item-title>
          <v-list-item-subtitle>{{ details.name }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Messages</v-list-item-title>
          <v-list-item-subtitle>{{ details.messageCount }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Consumers</v-list-item-title>
          <v-list-item-subtitle>{{ details.consumerCount }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Durable</v-list-item-title>
          <v-list-item-subtitle>{{ details.durable ? 'Yes' : 'No' }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Auto Delete</v-list-item-title>
          <v-list-item-subtitle>{{ details.autoDelete ? 'Yes' : 'No' }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Exclusive</v-list-item-title>
          <v-list-item-subtitle>{{ details.exclusive ? 'Yes' : 'No' }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>Memory</v-list-item-title>
          <v-list-item-subtitle>{{ formatBytes(details.memoryBytes) }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item v-if="details.arguments && Object.keys(details.arguments).length > 0">
          <v-list-item-title>Arguments</v-list-item-title>
          <v-list-item-subtitle>
            <pre>{{ JSON.stringify(details.arguments, null, 2) }}</pre>
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>
    </div>
    <div v-else>
      <v-progress-circular indeterminate color="primary" />
      <span class="ml-3">Loading queue details...</span>
    </div>
  </v-card-text>
  <v-card-actions>
    <v-spacer />
    <v-btn
      color="primary"
      @click="closeDialog"
    >
      Close
    </v-btn>
  </v-card-actions>
</template>

<script>
export default {
  name: 'QueueDetails',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    queueName: {
      type: String,
      required: true,
    },
  },
  async created() {
    await this.$store.dispatch('rabbitmq/loadQueueDetails', {
      serverUrl: this.serverUrl,
      queueName: this.queueName,
    });
  },
  computed: {
    details() {
      return this.$store.getters['rabbitmq/getQueueDetails'](
        this.serverUrl,
        this.queueName
      );
    },
  },
  methods: {
    formatBytes(bytes) {
      if (bytes === 0) return '0 Bytes';
      const k = 1024;
      const sizes = ['Bytes', 'KB', 'MB', 'GB'];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    },
    closeDialog() {
      this.$emit('close');
    },
  },
};
</script>

<style scoped>
pre {
  background-color: #f5f5f5;
  padding: 8px;
  border-radius: 4px;
  font-size: 12px;
}
</style>
