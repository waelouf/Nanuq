<template>
  <div class="row">
    <span class="mt-4">Topic Details - {{ topicDetails?.topicName || 'Loading...' }}</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item">
        <router-link to="/aws">AWS Servers</router-link>
      </li>
      <li class="breadcrumb-item">
        <router-link :to="{ name: 'ManageAWS', params: { serverId: serverId } }">
          Manage
        </router-link>
      </li>
      <li class="breadcrumb-item active">Topic Details</li>
    </ol>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <v-progress-circular
        indeterminate
        color="primary"
        size="64"
      />
    </div>

    <!-- Topic Attributes -->
    <div v-else-if="topicDetails">
      <v-card class="mb-4">
        <v-card-title>Topic Attributes</v-card-title>
        <v-card-text>
          <div class="mb-2"><strong>Topic Name:</strong> {{ topicDetails.topicName }}</div>
          <div class="mb-2">
            <strong>Topic ARN:</strong>
            <v-tooltip location="top">
              <template v-slot:activator="{ props }">
                <span v-bind="props" class="text-truncate" style="max-width: 600px; display: inline-block;">
                  {{ topicDetails.topicArn }}
                </span>
              </template>
              {{ topicDetails.topicArn }}
            </v-tooltip>
          </div>
          <div class="mb-2"><strong>Display Name:</strong> {{ topicDetails.displayName || 'N/A' }}</div>
          <div class="mb-2"><strong>Owner:</strong> {{ topicDetails.owner }}</div>
          <div class="mb-2"><strong>Subscriptions Confirmed:</strong> {{ topicDetails.subscriptionsConfirmed }}</div>
          <div class="mb-2"><strong>Subscriptions Pending:</strong> {{ topicDetails.subscriptionsPending }}</div>
          <div class="mb-2"><strong>Created:</strong> {{ formatDate(topicDetails.createdAt) }}</div>
        </v-card-text>
      </v-card>

      <!-- Publish Message Section -->
      <v-card class="mb-4">
        <v-card-title>Publish Message</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="subject"
            label="Subject"
            prepend-icon="mdi-text"
            :rules="[rules.required]"
          />

          <v-textarea
            v-model="messageBody"
            label="Message Body"
            rows="6"
            :rules="[rules.required]"
          />

          <v-btn
            color="primary"
            @click="publishMessage"
            :disabled="!messageBody || !subject || publishing"
            :loading="publishing"
            class="mt-2"
          >
            <v-icon start>mdi-publish</v-icon>
            Publish Message
          </v-btn>
        </v-card-text>
      </v-card>

      <!-- Subscriptions Section -->
      <v-card class="mb-4">
        <v-card-title class="d-flex justify-space-between align-center">
          <span>Subscriptions</span>
          <v-btn
            color="primary"
            size="small"
            @click="showSubscribeDialog = true"
          >
            <v-icon start>mdi-plus</v-icon>
            Subscribe
          </v-btn>
        </v-card-title>
        <v-card-text>
          <div class="card">
            <div class="datatable-wrapper datatable-loading no-footer">
              <div class="datatable-container">
                <table class="datatable-table">
                  <thead>
                    <tr>
                      <th>Protocol</th>
                      <th>Endpoint</th>
                      <th>Status</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="subscriptions.length === 0">
                      <td colspan="4" class="text-center text-muted py-4">
                        No subscriptions found. Click "Subscribe" to add a subscription.
                      </td>
                    </tr>
                    <tr v-for="(subscription, index) in subscriptions" :key="index">
                      <td>
                        <v-chip size="small" :color="getProtocolColor(subscription.protocol)">
                          {{ subscription.protocol }}
                        </v-chip>
                      </td>
                      <td>
                        <v-tooltip location="top">
                          <template v-slot:activator="{ props }">
                            <span v-bind="props" class="text-truncate" style="max-width: 300px; display: inline-block;">
                              {{ subscription.endpoint }}
                            </span>
                          </template>
                          {{ subscription.endpoint }}
                        </v-tooltip>
                      </td>
                      <td>
                        <v-chip
                          size="small"
                          :color="subscription.status === 'Confirmed' ? 'success' : 'warning'"
                        >
                          {{ subscription.status }}
                        </v-chip>
                      </td>
                      <td>
                        <a @click="confirmUnsubscribe(subscription)" class="delete-icon">
                          <i class="fa-regular fa-trash-can" />
                        </a>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </v-card-text>
      </v-card>
    </div>

    <!-- Subscribe Dialog -->
    <v-dialog v-model="showSubscribeDialog" width="600px">
      <v-card prepend-icon="mdi-bell-ring">
        <Subscribe
          :serverId="serverId"
          :topicArn="topicArn"
          @close="handleSubscribed"
        />
      </v-card>
    </v-dialog>

    <!-- Unsubscribe Confirmation Dialog -->
    <v-dialog v-model="showUnsubscribeDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Unsubscribe</v-card-title>
        <v-card-text>
          Are you sure you want to unsubscribe "{{ subscriptionToDelete?.endpoint }}"?
          This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showUnsubscribeDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="unsubscribe">
            Unsubscribe
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Refresh Credentials Dialog -->
    <v-dialog v-model="showRefreshCredentialsDialog" max-width="700px" persistent>
      <RefreshAwsCredentials
        :serverId="serverId"
        @close="showRefreshCredentialsDialog = false"
        @saved="handleCredentialsRefreshed"
      />
    </v-dialog>
  </div>
</template>

<script>
import Subscribe from './Subscribe.vue';
import RefreshAwsCredentials from '@/components/RefreshAwsCredentials.vue';

export default {
  name: 'TopicDetails',
  components: { Subscribe, RefreshAwsCredentials },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      loading: false,
      publishing: false,
      subject: '',
      messageBody: '',
      showSubscribeDialog: false,
      showUnsubscribeDialog: false,
      showRefreshCredentialsDialog: false,
      subscriptionToDelete: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  computed: {
    topicArn() {
      return this.$route.query.topicArn;
    },
    topicDetails() {
      return this.$store.getters['aws/getSnsTopicDetails'](this.serverId, this.topicArn);
    },
    subscriptions() {
      return this.$store.getters['aws/getSnsSubscriptions'](this.serverId, this.topicArn);
    },
  },
  async created() {
    await this.loadTopicDetails();
    await this.loadSubscriptions();
  },
  methods: {
    async loadTopicDetails() {
      this.loading = true;
      try {
        await this.$store.dispatch('aws/loadSnsTopicDetails', {
          serverId: this.serverId,
          topicArn: this.topicArn,
        });
      } catch (error) {
        // Check if this is an authentication error
        if (error.message === 'AWS_AUTH_ERROR') {
          this.showRefreshCredentialsDialog = true;
        }
        // Other errors handled by store
      } finally {
        this.loading = false;
      }
    },
    async loadSubscriptions() {
      try {
        await this.$store.dispatch('aws/loadSubscriptions', {
          serverId: this.serverId,
          topicArn: this.topicArn,
        });
      } catch (error) {
        // Check if this is an authentication error
        if (error.message === 'AWS_AUTH_ERROR') {
          this.showRefreshCredentialsDialog = true;
        }
        // Other errors handled by store
      }
    },
    async handleCredentialsRefreshed() {
      this.showRefreshCredentialsDialog = false;
      // Retry loading data with new credentials
      await this.loadTopicDetails();
      await this.loadSubscriptions();
    },
    async publishMessage() {
      this.publishing = true;
      try {
        await this.$store.dispatch('aws/publishMessage', {
          serverId: this.serverId,
          topicArn: this.topicArn,
          subject: this.subject,
          messageBody: this.messageBody,
        });
        this.subject = '';
        this.messageBody = '';
      } catch (error) {
        // Error handled by store
      } finally {
        this.publishing = false;
      }
    },
    handleSubscribed() {
      this.showSubscribeDialog = false;
      this.loadSubscriptions();
    },
    confirmUnsubscribe(subscription) {
      this.subscriptionToDelete = subscription;
      this.showUnsubscribeDialog = true;
    },
    async unsubscribe() {
      if (this.subscriptionToDelete) {
        try {
          await this.$store.dispatch('aws/unsubscribe', {
            serverId: this.serverId,
            topicArn: this.topicArn,
            subscriptionArn: this.subscriptionToDelete.subscriptionArn,
          });
        } catch (error) {
          // Error handled by store
        }
      }
      this.showUnsubscribeDialog = false;
      this.subscriptionToDelete = null;
    },
    getProtocolColor(protocol) {
      const colors = {
        email: 'primary',
        sms: 'warning',
        http: 'info',
        https: 'success',
        sqs: 'purple',
        lambda: 'orange',
      };
      return colors[protocol.toLowerCase()] || 'grey';
    },
    formatDate(date) {
      return new Date(date).toLocaleString();
    },
  },
};
</script>

<style scoped>
.delete-icon {
  color: blue;
  cursor: pointer;
}

.text-truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
