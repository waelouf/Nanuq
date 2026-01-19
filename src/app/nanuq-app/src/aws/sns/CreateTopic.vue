<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Create SNS Topic</v-card-title>

    <v-form fast-fail @submit.prevent="createTopic">
      <v-text-field
        v-model="topicName"
        label="Topic Name"
        prepend-icon="mdi-bullhorn-outline"
        :rules="[rules.required, rules.topicName]"
        hint="Topic name must be unique and can contain alphanumeric characters, hyphens, and underscores"
        persistent-hint
      />

      <v-text-field
        v-model="displayName"
        label="Display Name (optional)"
        prepend-icon="mdi-label"
        hint="Human-readable name for the topic"
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
      displayName: '',
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
        topicName: (value) => {
          const pattern = /^[a-zA-Z0-9_-]+$/;
          return pattern.test(value) || 'Topic name can only contain alphanumeric characters, hyphens, and underscores';
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
  methods: {
    async createTopic() {
      this.loading = true;
      this.error = null;

      try {
        const topicDetails = {
          serverId: this.serverId,
          region: this.region,
          topicName: this.topicName,
          displayName: this.displayName || this.topicName,
        };

        await this.$store.dispatch('aws/createTopic', topicDetails);
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
