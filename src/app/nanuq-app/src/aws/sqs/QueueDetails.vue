<template>
  <div class="row">
    <span class="mt-4">Queue Details - {{ queueDetails?.queueName || 'Loading...' }}</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item">
        <router-link to="/aws">AWS Servers</router-link>
      </li>
      <li class="breadcrumb-item">
        <router-link :to="{ name: 'ManageAWS', params: { serverId: serverId } }">
          Manage
        </router-link>
      </li>
      <li class="breadcrumb-item active">Queue Details</li>
    </ol>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <v-progress-circular
        indeterminate
        color="primary"
        size="64"
      />
    </div>

    <!-- Queue Attributes -->
    <div v-else-if="queueDetails">
      <v-card class="mb-4">
        <v-card-title>Queue Attributes</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="6">
              <div class="mb-2"><strong>Queue Name:</strong> {{ queueDetails.queueName }}</div>
              <div class="mb-2"><strong>Queue URL:</strong> {{ queueDetails.queueUrl }}</div>
              <div class="mb-2"><strong>FIFO Queue:</strong> {{ queueDetails.isFifo ? 'Yes' : 'No' }}</div>
              <div class="mb-2"><strong>Created:</strong> {{ formatDate(queueDetails.createdTimestamp) }}</div>
              <div class="mb-2"><strong>Last Modified:</strong> {{ formatDate(queueDetails.lastModifiedTimestamp) }}</div>
            </v-col>
            <v-col cols="12" md="6">
              <div class="mb-2"><strong>Approximate Messages:</strong> {{ queueDetails.approximateMessages }}</div>
              <div class="mb-2"><strong>Messages Not Visible:</strong> {{ queueDetails.approximateMessagesNotVisible }}</div>
              <div class="mb-2"><strong>Messages Delayed:</strong> {{ queueDetails.approximateMessagesDelayed }}</div>
              <div class="mb-2"><strong>Visibility Timeout:</strong> {{ queueDetails.visibilityTimeout }}s</div>
              <div class="mb-2"><strong>Message Retention:</strong> {{ queueDetails.messageRetentionPeriod }}s</div>
              <div class="mb-2"><strong>Max Message Size:</strong> {{ queueDetails.maximumMessageSize }} bytes</div>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>

      <!-- Send Message Section -->
      <v-card class="mb-4">
        <v-card-title>Send Message</v-card-title>
        <v-card-text>
          <v-textarea
            v-model="messageBody"
            label="Message Body"
            rows="4"
            :rules="[rules.required]"
          />

          <v-text-field
            v-model.number="delaySeconds"
            label="Delay (seconds)"
            type="number"
            hint="0-900 seconds"
          />

          <v-text-field
            v-if="queueDetails.isFifo"
            v-model="messageGroupId"
            label="Message Group ID"
            :rules="[rules.required]"
            hint="Required for FIFO queues"
          />

          <v-text-field
            v-if="queueDetails.isFifo"
            v-model="messageDeduplicationId"
            label="Message Deduplication ID (optional)"
            hint="Optional if content-based deduplication is enabled"
          />

          <v-btn
            color="primary"
            @click="sendMessage"
            :disabled="!messageBody || (queueDetails.isFifo && !messageGroupId) || sending"
            :loading="sending"
            class="mt-2"
          >
            <v-icon start>mdi-send</v-icon>
            Send Message
          </v-btn>
        </v-card-text>
      </v-card>

      <!-- Receive Messages Section -->
      <v-card class="mb-4">
        <v-card-title>Receive Messages</v-card-title>
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="maxMessages"
                label="Max Messages"
                type="number"
                hint="1-10"
                :rules="[rules.maxMessages]"
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="receiveVisibilityTimeout"
                label="Visibility Timeout (seconds)"
                type="number"
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="waitTimeSeconds"
                label="Wait Time (seconds)"
                type="number"
                hint="0-20 (0=short polling, 1-20=long polling)"
                :rules="[rules.waitTime]"
              />
            </v-col>
          </v-row>

          <v-btn
            color="primary"
            @click="receiveMessages"
            :loading="receiving"
            class="mb-4"
          >
            <v-icon start>mdi-download</v-icon>
            Receive Messages
          </v-btn>

          <!-- Messages Table -->
          <div v-if="messages.length > 0" class="card">
            <div class="datatable-wrapper datatable-loading no-footer">
              <div class="datatable-container">
                <table class="datatable-table">
                  <thead>
                    <tr>
                      <th>Message ID</th>
                      <th>Body</th>
                      <th>Timestamp</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(message, index) in messages" :key="index">
                      <td>
                        <v-tooltip location="top">
                          <template v-slot:activator="{ props }">
                            <span v-bind="props" class="text-truncate" style="max-width: 150px; display: inline-block;">
                              {{ message.messageId }}
                            </span>
                          </template>
                          {{ message.messageId }}
                        </v-tooltip>
                      </td>
                      <td>
                        <v-tooltip location="top">
                          <template v-slot:activator="{ props }">
                            <span v-bind="props" class="text-truncate" style="max-width: 300px; display: inline-block;">
                              {{ message.body }}
                            </span>
                          </template>
                          {{ message.body }}
                        </v-tooltip>
                      </td>
                      <td>{{ formatTimestamp(message.attributes.SentTimestamp) }}</td>
                      <td>
                        <a @click="confirmDeleteMessage(message)" class="delete-icon">
                          <i class="fa-regular fa-trash-can" />
                        </a>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          <div v-else class="text-center text-muted py-4">
            No messages received. Click "Receive Messages" to poll the queue.
          </div>
        </v-card-text>
      </v-card>
    </div>

    <!-- Delete Message Confirmation Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Message Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete this message? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showDeleteDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="deleteMessage">
            Delete
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
import RefreshAwsCredentials from '@/components/RefreshAwsCredentials.vue';

export default {
  name: 'QueueDetails',
  components: { RefreshAwsCredentials },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      loading: false,
      sending: false,
      receiving: false,
      messageBody: '',
      delaySeconds: 0,
      messageGroupId: '',
      messageDeduplicationId: '',
      maxMessages: 10,
      receiveVisibilityTimeout: 30,
      waitTimeSeconds: 0,
      showDeleteDialog: false,
      showRefreshCredentialsDialog: false,
      messageToDelete: null,
      rules: {
        required: (value) => !!value || 'Required',
        maxMessages: (value) => (value >= 1 && value <= 10) || 'Must be between 1 and 10',
        waitTime: (value) => (value >= 0 && value <= 20) || 'Must be between 0 and 20',
      },
    };
  },
  computed: {
    queueUrl() {
      return this.$route.query.queueUrl;
    },
    queueDetails() {
      return this.$store.getters['aws/getSqsQueueDetails'](this.serverId, this.queueUrl);
    },
    messages() {
      return this.$store.getters['aws/getSqsMessages'](this.serverId, this.queueUrl);
    },
  },
  async created() {
    await this.loadQueueDetails();
  },
  methods: {
    async loadQueueDetails() {
      this.loading = true;
      try {
        await this.$store.dispatch('aws/loadSqsQueueDetails', {
          serverId: this.serverId,
          queueUrl: this.queueUrl,
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
    async handleCredentialsRefreshed() {
      this.showRefreshCredentialsDialog = false;
      // Retry loading data with new credentials
      await this.loadQueueDetails();
    },
    async sendMessage() {
      this.sending = true;
      try {
        await this.$store.dispatch('aws/sendMessage', {
          serverId: this.serverId,
          queueUrl: this.queueUrl,
          messageBody: this.messageBody,
          delaySeconds: this.delaySeconds,
          messageGroupId: this.messageGroupId || null,
          messageDeduplicationId: this.messageDeduplicationId || null,
        });
        this.messageBody = '';
        this.delaySeconds = 0;
        this.messageGroupId = '';
        this.messageDeduplicationId = '';
        await this.loadQueueDetails();
      } catch (error) {
        // Error handled by store
      } finally {
        this.sending = false;
      }
    },
    async receiveMessages() {
      this.receiving = true;
      try {
        await this.$store.dispatch('aws/receiveMessages', {
          serverId: this.serverId,
          queueUrl: this.queueUrl,
          maxMessages: this.maxMessages,
          visibilityTimeout: this.receiveVisibilityTimeout,
          waitTimeSeconds: this.waitTimeSeconds,
        });
      } catch (error) {
        // Error handled by store
      } finally {
        this.receiving = false;
      }
    },
    confirmDeleteMessage(message) {
      this.messageToDelete = message;
      this.showDeleteDialog = true;
    },
    async deleteMessage() {
      if (this.messageToDelete) {
        try {
          await this.$store.dispatch('aws/deleteMessage', {
            serverId: this.serverId,
            queueUrl: this.queueUrl,
            receiptHandle: this.messageToDelete.receiptHandle,
          });
          await this.receiveMessages();
        } catch (error) {
          // Error handled by store
        }
      }
      this.showDeleteDialog = false;
      this.messageToDelete = null;
    },
    formatDate(date) {
      return new Date(date).toLocaleString();
    },
    formatTimestamp(timestamp) {
      return new Date(parseInt(timestamp, 10)).toLocaleString();
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
