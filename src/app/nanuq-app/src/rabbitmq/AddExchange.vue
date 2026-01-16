<template>
  <v-card-title>
    <span class="text-h5">Add Exchange</span>
  </v-card-title>
  <v-card-text>
    <v-form fast-fail @submit.prevent>
      <v-text-field
        v-model="exchangeName"
        label="Exchange Name"
        prepend-icon="mdi-swap-horizontal"
        :rules="[rules.required]"
      />

      <v-select
        v-model="exchangeType"
        label="Exchange Type"
        prepend-icon="mdi-format-list-bulleted-type"
        :items="exchangeTypes"
        :rules="[rules.required]"
      />

      <v-checkbox
        v-model="durable"
        label="Durable"
        hint="Exchange survives broker restart"
        persistent-hint
      />

      <v-checkbox
        v-model="autoDelete"
        label="Auto Delete"
        hint="Exchange is deleted when last binding is removed"
        persistent-hint
      />

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
      @click="addExchange"
      :disabled="!exchangeName || !exchangeType"
    >
      <v-icon start>mdi-plus</v-icon>
      Add Exchange
    </v-btn>
  </v-card-actions>
</template>

<script>
export default {
  name: 'AddExchange',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      exchangeName: '',
      exchangeType: 'direct',
      durable: true,
      autoDelete: false,
      exchangeTypes: ['direct', 'fanout', 'topic', 'headers'],
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  methods: {
    async addExchange() {
      const exchangeDetails = {
        serverUrl: this.serverUrl,
        name: this.exchangeName,
        type: this.exchangeType,
        durable: this.durable,
        autoDelete: this.autoDelete,
      };

      try {
        await this.$store.dispatch('rabbitmq/addExchange', exchangeDetails);
        setTimeout(() => {
          this.closeDialog();
        }, 1000);
      } catch (error) {
        // Error is already handled by the store
      }
    },
    closeDialog() {
      this.$emit('close');
    },
  },
};
</script>
