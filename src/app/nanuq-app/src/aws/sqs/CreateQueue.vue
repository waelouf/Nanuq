<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Create SQS Queue</v-card-title>

    <v-form fast-fail @submit.prevent="createQueue">
      <v-text-field
        v-model="queueName"
        label="Queue Name"
        prepend-icon="mdi-message-text"
        :rules="[rules.required, rules.queueName]"
        hint="Queue name must be unique and can contain alphanumeric characters, hyphens, and underscores"
        persistent-hint
      />

      <v-text-field
        v-model.number="visibilityTimeout"
        label="Visibility Timeout (seconds)"
        prepend-icon="mdi-timer"
        type="number"
        :rules="[rules.required, rules.positiveNumber]"
        hint="0-43200 seconds (12 hours)"
      />

      <v-text-field
        v-model.number="messageRetentionPeriod"
        label="Message Retention Period (seconds)"
        prepend-icon="mdi-clock-outline"
        type="number"
        :rules="[rules.required, rules.positiveNumber]"
        hint="60-1209600 seconds (1 minute to 14 days)"
      />

      <v-text-field
        v-model.number="maximumMessageSize"
        label="Maximum Message Size (bytes)"
        prepend-icon="mdi-file-outline"
        type="number"
        :rules="[rules.required, rules.positiveNumber]"
        hint="1024-262144 bytes (1KB to 256KB)"
      />

      <v-switch
        v-model="isFifo"
        label="FIFO Queue"
        color="primary"
        hint="First-In-First-Out queue (queue name must end with .fifo)"
        persistent-hint
      />

      <v-text-field
        v-if="!isFifo"
        v-model="deadLetterQueueArn"
        label="Dead Letter Queue ARN (optional)"
        prepend-icon="mdi-alert-circle-outline"
        hint="ARN of the dead-letter queue"
      />

      <v-text-field
        v-if="!isFifo && deadLetterQueueArn"
        v-model.number="maxReceiveCount"
        label="Max Receive Count"
        prepend-icon="mdi-counter"
        type="number"
        :rules="[rules.positiveNumber]"
        hint="Number of receives before sending to DLQ"
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
      visibilityTimeout: 30,
      messageRetentionPeriod: 345600, // 4 days
      maximumMessageSize: 262144, // 256 KB
      isFifo: false,
      deadLetterQueueArn: '',
      maxReceiveCount: 5,
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
        positiveNumber: (value) => value > 0 || 'Must be a positive number',
        queueName: (value) => {
          const pattern = /^[a-zA-Z0-9_-]+$/;
          return pattern.test(value) || 'Queue name can only contain alphanumeric characters, hyphens, and underscores';
        },
      },
    };
  },
  computed: {
    serverInfo() {
      const servers = this.$store.state.sqlite.awsServers || [];
      return servers.find((s) => s.id === parseInt(this.serverId, 10));
    },
    region() {
      return this.serverInfo?.region || '';
    },
  },
  watch: {
    isFifo(newVal) {
      if (newVal && !this.queueName.endsWith('.fifo')) {
        this.queueName = this.queueName ? `${this.queueName}.fifo` : '.fifo';
      } else if (!newVal && this.queueName.endsWith('.fifo')) {
        this.queueName = this.queueName.replace('.fifo', '');
      }
    },
  },
  methods: {
    async createQueue() {
      this.loading = true;
      this.error = null;

      try {
        const queueDetails = {
          serverId: this.serverId,
          region: this.region,
          queueName: this.queueName,
          visibilityTimeout: this.visibilityTimeout,
          messageRetentionPeriod: this.messageRetentionPeriod,
          maximumMessageSize: this.maximumMessageSize,
          isFifo: this.isFifo,
          deadLetterQueueArn: this.deadLetterQueueArn || null,
          maxReceiveCount: this.maxReceiveCount,
        };

        await this.$store.dispatch('aws/createQueue', queueDetails);
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
