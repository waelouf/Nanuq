<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Queue Details - {{ queue?.name || 'Loading...' }}</v-card-title>

    <v-tabs v-model="tab" color="primary">
      <v-tab value="properties">Properties</v-tab>
      <v-tab value="send">Send Message</v-tab>
      <v-tab value="receive">Receive Messages</v-tab>
    </v-tabs>

    <v-tabs-window v-model="tab" class="mt-4">
      <!-- Properties Tab -->
      <v-tabs-window-item value="properties">
        <v-card variant="flat">
          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <div class="mb-2"><strong>Queue Name:</strong> {{ queue.name }}</div>
                <div class="mb-2"><strong>Message Count:</strong> {{ queue.messageCount }}</div>
                <div class="mb-2"><strong>Dead Letter Count:</strong> {{ queue.deadLetterMessageCount }}</div>
                <div class="mb-2"><strong>Scheduled Messages:</strong> {{ queue.scheduledMessageCount }}</div>
                <div class="mb-2"><strong>Status:</strong> <v-chip size="small" :color="queue.status === 'Active' ? 'success' : 'warning'">{{ queue.status }}</v-chip></div>
              </v-col>
              <v-col cols="12" md="6">
                <div class="mb-2"><strong>Max Size:</strong> {{ queue.maxSizeInMegabytes }} MB</div>
                <div class="mb-2"><strong>Lock Duration:</strong> {{ formatTimeSpan(queue.lockDuration) }}</div>
                <div class="mb-2"><strong>Default TTL:</strong> {{ formatTimeSpan(queue.defaultMessageTimeToLive) }}</div>
                <div class="mb-2"><strong>Max Delivery Count:</strong> {{ queue.maxDeliveryCount }}</div>
                <div class="mb-2"><strong>Duplicate Detection:</strong> {{ queue.requiresDuplicateDetection ? 'Yes' : 'No' }}</div>
                <div class="mb-2"><strong>Requires Session:</strong> {{ queue.requiresSession ? 'Yes' : 'No' }}</div>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-tabs-window-item>

      <!-- Send Message Tab -->
      <v-tabs-window-item value="send">
        <v-card variant="flat">
          <v-card-text>
            <v-form @submit.prevent="sendMessage">
              <v-textarea
                v-model="messageBody"
                label="Message Body"
                rows="6"
                :rules="[rules.required]"
                hint="Enter the message content to send to the queue"
                persistent-hint
              />

              <v-text-field
                v-model="contentType"
                label="Content Type (optional)"
                prepend-icon="mdi-file-document-outline"
                hint="e.g., application/json, text/plain"
              />

              <v-textarea
                v-model="applicationPropertiesJson"
                label="Application Properties (JSON, optional)"
                rows="3"
                hint='e.g., {"key1": "value1", "key2": 123}'
              />

              <v-alert v-if="sendError" type="error" variant="tonal" class="mt-3">
                {{ sendError }}
              </v-alert>

              <v-btn
                color="primary"
                type="submit"
                :disabled="!messageBody || sending"
                :loading="sending"
                class="mt-2"
              >
                <v-icon start>mdi-send</v-icon>
                Send Message
              </v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-tabs-window-item>

      <!-- Receive Messages Tab -->
      <v-tabs-window-item value="receive">
        <v-card variant="flat">
          <v-card-text>
            <v-text-field
              v-model.number="maxMessages"
              label="Max Messages"
              type="number"
              prepend-icon="mdi-counter"
              hint="Maximum number of messages to receive (1-10)"
              :rules="[rules.maxMessages]"
              class="mb-4"
            />

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
                        <th>Content Type</th>
                        <th>Enqueued Time</th>
                        <th>Delivery Count</th>
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
                        <td>{{ message.contentType || 'N/A' }}</td>
                        <td>{{ formatDate(message.enqueuedTime) }}</td>
                        <td>
                          <v-chip size="small" :color="message.deliveryCount > 1 ? 'warning' : 'success'">
                            {{ message.deliveryCount }}
                          </v-chip>
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
      </v-tabs-window-item>
    </v-tabs-window>

    <div class="d-flex justify-end mt-4">
      <v-btn color="secondary" variant="outlined" @click="close">
        Close
      </v-btn>
    </div>
  </v-sheet>
</template>

<script>
export default {
  name: 'QueueDetails',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
    queue: {
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
      maxMessages: 10,
      messages: [],
      sending: false,
      receiving: false,
      sendError: null,
      rules: {
        required: (value) => !!value || 'Required',
        maxMessages: (value) => (value >= 1 && value <= 10) || 'Must be between 1 and 10',
      },
    };
  },
  methods: {
    async sendMessage() {
      this.sending = true;
      this.sendError = null;

      try {
        let applicationProperties = null;
        if (this.applicationPropertiesJson) {
          try {
            applicationProperties = JSON.parse(this.applicationPropertiesJson);
          } catch (e) {
            this.sendError = 'Invalid JSON in Application Properties';
            this.sending = false;
            return;
          }
        }

        const messageDetails = {
          serverId: parseInt(this.serverId, 10),
          queueName: this.queue.name,
          messageBody: this.messageBody,
          contentType: this.contentType || null,
          applicationProperties,
        };

        await this.$store.dispatch('azure/sendMessage', messageDetails);
        this.messageBody = '';
        this.contentType = '';
        this.applicationPropertiesJson = '';
      } catch (error) {
        this.sendError = 'Failed to send message. Please try again.';
      } finally {
        this.sending = false;
      }
    },
    async receiveMessages() {
      this.receiving = true;

      try {
        const messages = await this.$store.dispatch('azure/receiveMessages', {
          serverId: parseInt(this.serverId, 10),
          queueName: this.queue.name,
        });
        this.messages = messages;
      } catch (error) {
        // Error handled by store
      } finally {
        this.receiving = false;
      }
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
    formatDate(dateString) {
      if (!dateString) return 'N/A';
      const date = new Date(dateString);
      return date.toLocaleString();
    },
    close() {
      this.$emit('close');
    },
  },
};
</script>

<style scoped>
.text-truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
