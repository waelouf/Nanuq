<template>
  <v-card-title>
    <span class="text-h5">Add Queue</span>
  </v-card-title>
  <v-card-text>
    <v-form fast-fail @submit.prevent>
      <v-text-field
        v-model="queueName"
        label="Queue Name"
        prepend-icon="mdi-format-list-bulleted"
        :rules="[rules.required]"
      />

      <v-checkbox
        v-model="durable"
        label="Durable"
        hint="Queue survives broker restart"
        persistent-hint
      />

      <v-checkbox
        v-model="autoDelete"
        label="Auto Delete"
        hint="Queue is deleted when last consumer unsubscribes"
        persistent-hint
      />

      <v-checkbox
        v-model="exclusive"
        label="Exclusive"
        hint="Queue is used by only one connection and deleted when that connection closes"
        persistent-hint
      />

      <v-alert
        v-if="errorMessage"
        type="error"
        variant="tonal"
        class="mt-3"
      >
        {{ errorMessage }}
      </v-alert>

      <v-alert
        v-if="successMessage"
        type="success"
        variant="tonal"
        class="mt-3"
      >
        {{ successMessage }}
      </v-alert>
    </v-form>
  </v-card-text>
  <v-card-actions>
    <v-spacer />
    <v-btn
      color="secondary"
      variant="outlined"
      @click="closeDialog"
    >
      Cancel
    </v-btn>
    <v-btn
      color="primary"
      @click="addQueue"
      :disabled="!queueName"
    >
      <v-icon start>mdi-plus</v-icon>
      Add Queue
    </v-btn>
  </v-card-actions>
</template>

<script>
export default {
  name: 'AddQueue',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      queueName: '',
      durable: true,
      autoDelete: false,
      exclusive: false,
      errorMessage: '',
      successMessage: '',
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  methods: {
    async addQueue() {
      this.errorMessage = '';
      this.successMessage = '';

      const queueDetails = {
        serverUrl: this.serverUrl,
        name: this.queueName,
        durable: this.durable,
        autoDelete: this.autoDelete,
        exclusive: this.exclusive,
      };

      try {
        await this.$store.dispatch('rabbitmq/addQueue', queueDetails);
        this.successMessage = 'Queue added successfully!';
        setTimeout(() => {
          this.closeDialog();
        }, 1000);
      } catch (error) {
        this.errorMessage = `Failed to add queue: ${error.message}`;
      }
    },
    closeDialog() {
      this.$emit('close');
    },
  },
};
</script>
