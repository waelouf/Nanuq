<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Subscribe to Topic</v-card-title>

    <v-form fast-fail @submit.prevent="subscribe">
      <v-select
        v-model="protocol"
        label="Protocol"
        prepend-icon="mdi-connection"
        :items="protocols"
        :rules="[rules.required]"
        hint="Choose the subscription protocol"
        persistent-hint
      />

      <v-text-field
        v-model="endpoint"
        :label="getEndpointLabel()"
        prepend-icon="mdi-target"
        :rules="[rules.required, getEndpointRule()]"
        :hint="getEndpointHint()"
        persistent-hint
      />

      <v-alert v-if="error" type="error" variant="tonal" class="mt-3">
        {{ error }}
      </v-alert>

      <v-alert v-if="protocol === 'email' || protocol === 'email-json'" type="info" variant="tonal" class="mt-3">
        Email subscriptions require confirmation. The recipient will receive a confirmation email.
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
          :disabled="!protocol || !endpoint || loading"
          :loading="loading"
        >
          <v-icon start>mdi-bell-ring</v-icon>
          Subscribe
        </v-btn>
      </div>
    </v-form>
  </v-sheet>
</template>

<script>
export default {
  name: 'Subscribe',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
    topicArn: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      protocol: 'email',
      endpoint: '',
      protocols: [
        { title: 'Email', value: 'email' },
        { title: 'Email (JSON)', value: 'email-json' },
        { title: 'SMS', value: 'sms' },
        { title: 'HTTP', value: 'http' },
        { title: 'HTTPS', value: 'https' },
        { title: 'SQS', value: 'sqs' },
        { title: 'Lambda', value: 'lambda' },
      ],
      loading: false,
      error: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  methods: {
    getEndpointLabel() {
      const labels = {
        email: 'Email Address',
        'email-json': 'Email Address',
        sms: 'Phone Number',
        http: 'HTTP Endpoint URL',
        https: 'HTTPS Endpoint URL',
        sqs: 'SQS Queue ARN',
        lambda: 'Lambda Function ARN',
      };
      return labels[this.protocol] || 'Endpoint';
    },
    getEndpointHint() {
      const hints = {
        email: 'example@example.com',
        'email-json': 'example@example.com (receives JSON format)',
        sms: '+1234567890',
        http: 'http://example.com/endpoint',
        https: 'https://example.com/endpoint',
        sqs: 'arn:aws:sqs:region:account-id:queue-name',
        lambda: 'arn:aws:lambda:region:account-id:function:function-name',
      };
      return hints[this.protocol] || '';
    },
    getEndpointRule() {
      return (value) => {
        if (!value) return 'Required';

        switch (this.protocol) {
          case 'email':
          case 'email-json':
            // Simple email validation
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || 'Invalid email address';
          case 'sms':
            // Phone number with + prefix
            return /^\+\d{10,15}$/.test(value) || 'Invalid phone number (use format: +1234567890)';
          case 'http':
            return value.startsWith('http://') || 'URL must start with http://';
          case 'https':
            return value.startsWith('https://') || 'URL must start with https://';
          case 'sqs':
          case 'lambda':
            return value.startsWith('arn:aws:') || 'Invalid ARN format';
          default:
            return true;
        }
      };
    },
    async subscribe() {
      this.loading = true;
      this.error = null;

      try {
        const subscriptionDetails = {
          serverId: this.serverId,
          topicArn: this.topicArn,
          protocol: this.protocol,
          endpoint: this.endpoint,
        };

        await this.$store.dispatch('aws/subscribe', subscriptionDetails);
        this.close();
      } catch (error) {
        this.error = 'Failed to subscribe. Please check your inputs and try again.';
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
