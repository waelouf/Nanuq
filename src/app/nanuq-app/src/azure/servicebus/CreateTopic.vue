<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Create Azure Service Bus Topic</v-card-title>

    <v-form fast-fail @submit.prevent="createTopic">
      <v-text-field
        v-model="topicName"
        label="Topic Name"
        prepend-icon="mdi-bullhorn-outline"
        :rules="[rules.required, rules.topicName]"
        hint="Topic name must be unique and can contain letters, numbers, periods, hyphens, and underscores"
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
        v-model="supportOrdering"
        label="Support Ordering"
        color="primary"
        hint="Enable message ordering within topics"
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
          :disabled="!topicName || loading"
          :loading="loading"
        >
          <v-icon start>mdi-plus</v-icon>
          Create Topic
        </v-btn>
      </div>
    </v-form>
  </v-sheet>
</template>

<script>
export default {
  name: 'CreateTopic',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      topicName: '',
      maxSizeInMegabytes: 1024,
      defaultMessageTtlSeconds: '',
      requiresDuplicateDetection: false,
      supportOrdering: false,
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
        positiveNumber: (value) => value > 0 || 'Must be a positive number',
        topicName: (value) => {
          const pattern = /^[a-zA-Z0-9._-]+$/;
          return pattern.test(value) || 'Topic name can only contain letters, numbers, periods, hyphens, and underscores';
        },
      },
    };
  },
  methods: {
    async createTopic() {
      this.loading = true;
      this.error = null;

      try {
        const topicDetails = {
          serverId: parseInt(this.serverId, 10),
          topicName: this.topicName,
          maxSizeInMegabytes: this.maxSizeInMegabytes,
          requiresDuplicateDetection: this.requiresDuplicateDetection,
          supportOrdering: this.supportOrdering,
        };

        // Add optional field only if it has a value
        if (this.defaultMessageTtlSeconds) {
          topicDetails.defaultMessageTimeToLive = `PT${this.defaultMessageTtlSeconds}S`;
        }

        await this.$store.dispatch('azure/createTopic', topicDetails);
        this.close();
      } catch (error) {
        this.error = 'Failed to create topic. Please check your inputs and try again.';
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
