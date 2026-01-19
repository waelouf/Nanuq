<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Create Subscription</v-card-title>

    <v-form fast-fail @submit.prevent="createSubscription">
      <v-text-field
        v-model="subscriptionName"
        label="Subscription Name"
        prepend-icon="mdi-rss"
        :rules="[rules.required, rules.subscriptionName]"
        hint="Subscription name must be unique and can contain letters, numbers, periods, hyphens, and underscores"
        persistent-hint
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
          :disabled="!subscriptionName || loading"
          :loading="loading"
        >
          <v-icon start>mdi-plus</v-icon>
          Create Subscription
        </v-btn>
      </div>
    </v-form>
  </v-sheet>
</template>

<script>
export default {
  name: 'CreateSubscription',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
    topicName: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      subscriptionName: '',
      maxDeliveryCount: 10,
      lockDurationSeconds: '',
      requiresSession: false,
      deadLetteringOnMessageExpiration: false,
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
        positiveNumber: (value) => value > 0 || 'Must be a positive number',
        subscriptionName: (value) => {
          const pattern = /^[a-zA-Z0-9._-]+$/;
          return pattern.test(value) || 'Subscription name can only contain letters, numbers, periods, hyphens, and underscores';
        },
      },
    };
  },
  methods: {
    async createSubscription() {
      this.loading = true;
      this.error = null;

      try {
        const subscriptionDetails = {
          serverId: parseInt(this.serverId, 10),
          topicName: this.topicName,
          subscriptionName: this.subscriptionName,
          maxDeliveryCount: this.maxDeliveryCount,
          requiresSession: this.requiresSession,
          deadLetteringOnMessageExpiration: this.deadLetteringOnMessageExpiration,
        };

        // Add optional field only if it has a value
        if (this.lockDurationSeconds) {
          subscriptionDetails.lockDuration = `PT${this.lockDurationSeconds}S`;
        }

        await this.$store.dispatch('azure/createSubscription', subscriptionDetails);
        this.close();
      } catch (error) {
        this.error = 'Failed to create subscription. Please check your inputs and try again.';
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
