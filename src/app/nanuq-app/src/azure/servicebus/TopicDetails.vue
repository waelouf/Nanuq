<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Topic Details - {{ topic?.name || 'Loading...' }}</v-card-title>

    <v-tabs v-model="tab" color="primary">
      <v-tab value="properties">Properties</v-tab>
      <v-tab value="publish">Publish Message</v-tab>
      <v-tab value="subscriptions">Subscriptions</v-tab>
    </v-tabs>

    <v-tabs-window v-model="tab" class="mt-4">
      <!-- Properties Tab -->
      <v-tabs-window-item value="properties">
        <v-card variant="flat">
          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <div class="mb-2"><strong>Topic Name:</strong> {{ topic.name }}</div>
                <div class="mb-2"><strong>Subscription Count:</strong> {{ topic.subscriptionCount }}</div>
                <div class="mb-2"><strong>Status:</strong> <v-chip size="small" :color="topic.status === 'Active' ? 'success' : 'warning'">{{ topic.status }}</v-chip></div>
              </v-col>
              <v-col cols="12" md="6">
                <div class="mb-2"><strong>Max Size:</strong> {{ topic.maxSizeInMegabytes }} MB</div>
                <div class="mb-2"><strong>Default TTL:</strong> {{ formatTimeSpan(topic.defaultMessageTimeToLive) }}</div>
                <div class="mb-2"><strong>Duplicate Detection:</strong> {{ topic.requiresDuplicateDetection ? 'Yes' : 'No' }}</div>
                <div class="mb-2"><strong>Support Ordering:</strong> {{ topic.supportOrdering ? 'Yes' : 'No' }}</div>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-tabs-window-item>

      <!-- Publish Message Tab -->
      <v-tabs-window-item value="publish">
        <v-card variant="flat">
          <v-card-text>
            <v-form @submit.prevent="publishMessage">
              <!-- Message Body Section -->
              <div class="mb-6">
                <v-textarea
                  v-model="messageBody"
                  label="Message Body *"
                  rows="6"
                  :rules="[rules.required]"
                  hint="Enter the message content to publish to the topic"
                  persistent-hint
                  variant="outlined"
                  autofocus
                  color="primary"
                />
              </div>

              <!-- Optional Fields Section -->
              <div class="mb-4">
                <v-text-field
                  v-model="contentType"
                  label="Content Type (optional)"
                  prepend-icon="mdi-file-document-outline"
                  hint="e.g., application/json, text/plain"
                  variant="outlined"
                  color="primary"
                />
              </div>

              <div class="mb-6 app-properties-wrapper">
                <v-textarea
                  v-model="applicationPropertiesJson"
                  label="Application Properties (JSON, optional)"
                  rows="3"
                  auto-grow
                  hint='e.g., {"key1": "value1", "key2": 123}'
                  persistent-hint
                  variant="outlined"
                  color="primary"
                  placeholder='{"key": "value"}'
                  class="app-properties-field"
                />
              </div>

              <v-divider class="mb-4" />

              <v-alert v-if="publishError" type="error" variant="tonal" class="mb-3">
                {{ publishError }}
              </v-alert>

              <v-alert v-if="!messageBody && !publishing" type="info" variant="tonal" class="mb-3">
                Please enter a message body to enable publishing
              </v-alert>

              <v-btn
                color="primary"
                type="submit"
                :disabled="!messageBody || publishing"
                :loading="publishing"
                size="large"
              >
                <v-icon start>mdi-publish</v-icon>
                Publish Message
              </v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-tabs-window-item>

      <!-- Subscriptions Tab -->
      <v-tabs-window-item value="subscriptions">
        <v-card variant="flat">
          <v-card-text>
            <div class="card mb-4">
              <div class="datatable-wrapper datatable-loading no-footer">
                <div class="datatable-container">
                  <table class="datatable-table">
                    <thead>
                      <tr>
                        <th>Subscription Name</th>
                        <th>Messages</th>
                        <th>Dead Letter</th>
                        <th>Max Delivery</th>
                        <th>Status</th>
                        <th />
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="subscriptions.length === 0">
                        <td colspan="6" class="text-center text-muted py-4">
                          No subscriptions found. Create a subscription to start receiving messages.
                        </td>
                      </tr>
                      <tr v-for="(subscription, index) in subscriptions" :key="index">
                        <td>{{ subscription.subscriptionName }}</td>
                        <td>
                          <v-chip size="small" color="primary">
                            {{ subscription.messageCount }}
                          </v-chip>
                        </td>
                        <td>
                          <v-chip size="small" :color="subscription.deadLetterMessageCount > 0 ? 'error' : 'grey'">
                            {{ subscription.deadLetterMessageCount }}
                          </v-chip>
                        </td>
                        <td>{{ subscription.maxDeliveryCount }}</td>
                        <td>
                          <v-chip size="small" :color="subscription.status === 'Active' ? 'success' : 'warning'">
                            {{ subscription.status }}
                          </v-chip>
                        </td>
                        <td>
                          <a @click="confirmDeleteSubscription(subscription)" class="delete-icon">
                            <i class="fa-regular fa-trash-can" />
                          </a>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>

            <v-btn
              color="primary"
              @click="showCreateSubscriptionDialog = true"
              class="mt-2"
              prepend-icon="mdi-plus"
            >
              Create Subscription
            </v-btn>
          </v-card-text>
        </v-card>
      </v-tabs-window-item>
    </v-tabs-window>

    <div class="d-flex justify-end mt-4">
      <v-btn color="secondary" variant="outlined" @click="close">
        Close
      </v-btn>
    </div>

    <!-- Create Subscription Dialog -->
    <v-dialog v-model="showCreateSubscriptionDialog" width="600px">
      <v-card prepend-icon="mdi-rss">
        <CreateSubscription
          :server-id="serverId"
          :topic-name="topic.name"
          @close="handleSubscriptionCreated"
        />
      </v-card>
    </v-dialog>

    <!-- Delete Subscription Confirmation Dialog -->
    <v-dialog v-model="showDeleteSubscriptionDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Subscription Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the subscription "{{ subscriptionToDelete?.subscriptionName }}"?
          This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showDeleteSubscriptionDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="deleteSubscription">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
import CreateSubscription from './CreateSubscription.vue';

export default {
  name: 'TopicDetails',
  components: { CreateSubscription },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
    topic: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      tab: 'properties',
      messageBody: '',
      contentType: '',
      applicationPropertiesJson: '',
      subscriptions: [],
      showCreateSubscriptionDialog: false,
      showDeleteSubscriptionDialog: false,
      subscriptionToDelete: null,
      publishing: false,
      publishError: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  async created() {
    await this.loadSubscriptions();
  },
  methods: {
    async loadSubscriptions() {
      try {
        await this.$store.dispatch('azure/loadSubscriptions', {
          serverId: parseInt(this.serverId, 10),
          topicName: this.topic.name,
        });
        this.subscriptions = this.$store.state.azure.subscriptions || [];
      } catch (error) {
        // Error handled by store
      }
    },
    async publishMessage() {
      this.publishing = true;
      this.publishError = null;

      try {
        let applicationProperties = null;
        if (this.applicationPropertiesJson) {
          try {
            applicationProperties = JSON.parse(this.applicationPropertiesJson);
          } catch (e) {
            this.publishError = 'Invalid JSON in Application Properties';
            this.publishing = false;
            return;
          }
        }

        const messageDetails = {
          serverId: parseInt(this.serverId, 10),
          topicName: this.topic.name,
          messageBody: this.messageBody,
          contentType: this.contentType || null,
          applicationProperties,
        };

        await this.$store.dispatch('azure/publishMessage', messageDetails);
        this.messageBody = '';
        this.contentType = '';
        this.applicationPropertiesJson = '';
      } catch (error) {
        this.publishError = 'Failed to publish message. Please try again.';
      } finally {
        this.publishing = false;
      }
    },
    handleSubscriptionCreated() {
      this.showCreateSubscriptionDialog = false;
      this.loadSubscriptions();
    },
    confirmDeleteSubscription(subscription) {
      this.subscriptionToDelete = subscription;
      this.showDeleteSubscriptionDialog = true;
    },
    async deleteSubscription() {
      if (this.subscriptionToDelete) {
        try {
          await this.$store.dispatch('azure/deleteSubscription', {
            serverId: parseInt(this.serverId, 10),
            topicName: this.topic.name,
            subscriptionName: this.subscriptionToDelete.subscriptionName,
          });
          await this.loadSubscriptions();
        } catch (error) {
          // Error handled by store
        }
      }
      this.showDeleteSubscriptionDialog = false;
      this.subscriptionToDelete = null;
    },
    formatTimeSpan(timeSpan) {
      if (!timeSpan) return 'N/A';
      // Parse timespan format like "00:01:00" (HH:MM:SS)
      const match = timeSpan.match(/(\d+):(\d+):(\d+)/);
      if (match) {
        const hours = parseInt(match[1], 10);
        const minutes = parseInt(match[2], 10);
        const seconds = parseInt(match[3], 10);
        if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
        if (minutes > 0) return `${minutes}m ${seconds}s`;
        return `${seconds}s`;
      }
      return timeSpan;
    },
    close() {
      this.$emit('close');
    },
  },
};
</script>

<style scoped>
.delete-icon {
  color: blue;
  cursor: pointer;
}

/* Ensure Application Properties textarea is visible */
.app-properties-wrapper {
  min-height: 120px;
}

.app-properties-field {
  display: block !important;
  visibility: visible !important;
  min-height: 100px !important;
}

.app-properties-field :deep(textarea) {
  min-height: 80px !important;
  display: block !important;
  visibility: visible !important;
}

.app-properties-field :deep(.v-field__field) {
  min-height: 100px !important;
}

.app-properties-field :deep(.v-field__input) {
  min-height: 80px !important;
}
</style>
