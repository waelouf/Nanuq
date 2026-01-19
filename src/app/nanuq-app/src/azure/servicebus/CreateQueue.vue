<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Create Azure Service Bus Queue</v-card-title>

    <v-form fast-fail @submit.prevent="createQueue">
      <v-text-field
        v-model="queueName"
        label="Queue Name"
        prepend-icon="mdi-message-text"
        :rules="[rules.required, rules.queueName]"
        hint="Queue name must be unique and can contain letters, numbers, periods, hyphens, and underscores"
        persistent-hint
      />

      <v-text-field
        v-model.number="maxSizeInMegabytes"
        label="Max Size (MB)"
        prepend-icon="mdi-database"
        type="number"
        :rules="[rules.required, rules.positiveNumber]"
        hint="1-5120 MB (up to 5GB for premium)"
      />

      <v-text-field
        v-model.number="maxDeliveryCount"
        label="Max Delivery Count"
        prepend-icon="mdi-counter"
        type="number"
        :rules="[rules.required, rules.positiveNumber]"
        hint="Number of receives before moving to dead-letter queue (default: 10)"
      />

      <v-text-field
        v-model="lockDurationSeconds"
        label="Lock Duration (seconds, optional)"
        prepend-icon="mdi-lock-clock"
        type="number"
        hint="30-300 seconds (leave blank for default 60s)"
      />

      <v-text-field
        v-model="defaultMessageTtlSeconds"
        label="Default Message TTL (seconds, optional)"
        prepend-icon="mdi-clock-outline"
        type="number"
        hint="Message time-to-live (leave blank for unlimited)"
      />

      <v-switch
        v-model="requiresDuplicateDetection"
        label="Requires Duplicate Detection"
        color="primary"
        hint="Enable duplicate message detection"
        persistent-hint
      />

      <v-switch
        v-model="requiresSession"
        label="Requires Session"
        color="primary"
        hint="Enable session support for ordered message processing"
        persistent-hint
      />

      <v-switch
        v-model="deadLetteringOnMessageExpiration"
        label="Dead Lettering on Message Expiration"
        color="primary"
        hint="Move expired messages to dead-letter queue"
        persistent-hint
      />

      <v-alert v-if="error" type="error" variant="tonal" class="mt-3">
        {{ error }}
      </v-alert>

      <div class="d-flex justify-end gap-2 mt-4">
        <v-btn
          color="secondary"
          variant="outlined"
          @click="close"
        >
          Cancel
        </v-btn>
        <v-btn
          color="primary"
          type="submit"
          :disabled="!queueName || loading"
          :loading="loading"
        >
          <v-icon start>mdi-plus</v-icon>
          Create Queue
        </v-btn>
      </div>
    </v-form>
  </v-sheet>
</template>

<script>
export default {
  name: 'CreateQueue',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      queueName: '',
      maxSizeInMegabytes: 1024,
      maxDeliveryCount: 10,
      lockDurationSeconds: '',
      defaultMessageTtlSeconds: '',
      requiresDuplicateDetection: false,
      requiresSession: false,
      deadLetteringOnMessageExpiration: false,
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
        positiveNumber: (value) => value > 0 || 'Must be a positive number',
        queueName: (value) => {
          const pattern = /^[a-zA-Z0-9._-]+$/;
          return pattern.test(value) || 'Queue name can only contain letters, numbers, periods, hyphens, and underscores';
        },
      },
    };
  },
  methods: {
    async createQueue() {
      this.loading = true;
      this.error = null;

      try {
        const queueDetails = {
          serverId: parseInt(this.serverId, 10),
          queueName: this.queueName,
          maxSizeInMegabytes: this.maxSizeInMegabytes,
          maxDeliveryCount: this.maxDeliveryCount,
          requiresDuplicateDetection: this.requiresDuplicateDetection,
          requiresSession: this.requiresSession,
          deadLetteringOnMessageExpiration: this.deadLetteringOnMessageExpiration,
        };

        // Add optional fields only if they have values
        if (this.lockDurationSeconds) {
          queueDetails.lockDuration = `PT${this.lockDurationSeconds}S`;
        }

        if (this.defaultMessageTtlSeconds) {
          queueDetails.defaultMessageTimeToLive = `PT${this.defaultMessageTtlSeconds}S`;
        }

        await this.$store.dispatch('azure/createQueue', queueDetails);
        this.close();
      } catch (error) {
        this.error = 'Failed to create queue. Please check your inputs and try again.';
      } finally {
        this.loading = false;
      }
    },
    close() {
      this.$emit('close');
    },
  },
};
</script>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
