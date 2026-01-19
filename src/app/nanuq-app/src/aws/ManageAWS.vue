<template>
  <div class="row">
    <span class="mt-4">Manage AWS - {{ serverInfo?.alias || 'Loading...' }}</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active">
        <router-link to="/aws">AWS Servers</router-link>
      </li>
      <li class="breadcrumb-item active">{{ serverInfo?.region || '' }}</li>
    </ol>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <v-progress-circular
        indeterminate
        color="primary"
        size="64"
      />
    </div>

    <!-- Error State -->
    <v-alert v-else-if="error" type="error" variant="tonal" class="mb-4">
      {{ error }}
    </v-alert>

    <!-- Main Content -->
    <div v-else>
      <v-tabs v-model="tab" class="mb-4">
        <v-tab value="sqs">
          <v-icon start>mdi-message-text-outline</v-icon>
          SQS Queues
        </v-tab>
        <v-tab value="sns">
          <v-icon start>mdi-bullhorn-outline</v-icon>
          SNS Topics
        </v-tab>
      </v-tabs>

      <v-tabs-window v-model="tab">
        <!-- SQS Queues Tab -->
        <v-tabs-window-item value="sqs">
          <div class="card mb-4">
            <div class="datatable-wrapper datatable-loading no-footer">
              <div class="datatable-container">
                <table id="queues-table" class="datatable-table">
                  <thead>
                    <tr>
                      <th>Queue Name</th>
                      <th>Messages</th>
                      <th>Queue URL</th>
                      <th />
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="sqsQueues.length === 0">
                      <td colspan="5" class="text-center text-muted py-4">
                        No queues found. Create your first queue to get started.
                      </td>
                    </tr>
                    <tr
                      v-for="(queue, index) in sqsQueues"
                      :key="index"
                    >
                      <td>{{ queue.queueName }}</td>
                      <td>
                        <v-chip size="small" color="primary">
                          {{ queue.approximateMessages }}
                        </v-chip>
                      </td>
                      <td>
                        <v-tooltip location="top">
                          <template v-slot:activator="{ props }">
                            <span v-bind="props" class="text-truncate" style="max-width: 300px; display: inline-block;">
                              {{ queue.queueUrl }}
                            </span>
                          </template>
                          {{ queue.queueUrl }}
                        </v-tooltip>
                      </td>
                      <td>
                        <router-link
                          :to="{
                            name: 'SQSQueueDetails',
                            params: {
                              serverId: serverId,
                            },
                            query: {
                              queueUrl: queue.queueUrl,
                            },
                          }"
                          class="detail-link"
                        >
                          Details
                        </router-link>
                      </td>
                      <td>
                        <a @click="confirmDeleteQueue(queue)" class="delete-icon">
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
            class="mt-2"
            @click="showCreateQueueDialog = true"
            type="submit"
            block
            prepend-icon="mdi-plus"
          >
            Create Queue
          </v-btn>
        </v-tabs-window-item>

        <!-- SNS Topics Tab -->
        <v-tabs-window-item value="sns">
          <div class="card mb-4">
            <div class="datatable-wrapper datatable-loading no-footer">
              <div class="datatable-container">
                <table id="topics-table" class="datatable-table">
                  <thead>
                    <tr>
                      <th>Topic Name</th>
                      <th>Topic ARN</th>
                      <th>Subscriptions</th>
                      <th />
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="snsTopics.length === 0">
                      <td colspan="5" class="text-center text-muted py-4">
                        No topics found. Create your first topic to get started.
                      </td>
                    </tr>
                    <tr
                      v-for="(topic, index) in snsTopics"
                      :key="index"
                    >
                      <td>{{ topic.topicName }}</td>
                      <td>
                        <v-tooltip location="top">
                          <template v-slot:activator="{ props }">
                            <span v-bind="props" class="text-truncate" style="max-width: 300px; display: inline-block;">
                              {{ topic.topicArn }}
                            </span>
                          </template>
                          {{ topic.topicArn }}
                        </v-tooltip>
                      </td>
                      <td>
                        <v-chip size="small" color="success">
                          {{ topic.subscriptionsCount }}
                        </v-chip>
                      </td>
                      <td>
                        <router-link
                          :to="{
                            name: 'SNSTopicDetails',
                            params: {
                              serverId: serverId,
                            },
                            query: {
                              topicArn: topic.topicArn,
                            },
                          }"
                          class="detail-link"
                        >
                          Details
                        </router-link>
                      </td>
                      <td>
                        <a @click="confirmDeleteTopic(topic)" class="delete-icon">
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
            class="mt-2"
            @click="showCreateTopicDialog = true"
            type="submit"
            block
            prepend-icon="mdi-plus"
          >
            Create Topic
          </v-btn>
        </v-tabs-window-item>
      </v-tabs-window>
    </div>

    <!-- Create Queue Dialog -->
    <v-dialog v-model="showCreateQueueDialog" width="600px">
      <v-card prepend-icon="mdi-message-text-outline">
        <CreateQueue :serverId="serverId" @close="handleQueueCreated" />
      </v-card>
    </v-dialog>

    <!-- Create Topic Dialog -->
    <v-dialog v-model="showCreateTopicDialog" width="600px">
      <v-card prepend-icon="mdi-bullhorn-outline">
        <CreateTopic :serverId="serverId" @close="handleTopicCreated" />
      </v-card>
    </v-dialog>

    <!-- Delete Queue Confirmation Dialog -->
    <v-dialog v-model="showDeleteQueueDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Queue Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the queue "{{ queueToDelete?.queueName }}"?
          This action cannot be undone and all messages will be lost.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showDeleteQueueDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="deleteQueue">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Topic Confirmation Dialog -->
    <v-dialog v-model="showDeleteTopicDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Topic Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the topic "{{ topicToDelete?.topicName }}"?
          This action cannot be undone and all subscriptions will be removed.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showDeleteTopicDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="deleteTopic">
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
import CreateQueue from './sqs/CreateQueue.vue';
import CreateTopic from './sns/CreateTopic.vue';
import RefreshAwsCredentials from '@/components/RefreshAwsCredentials.vue';

export default {
  name: 'ManageAWS',
  components: { CreateQueue, CreateTopic, RefreshAwsCredentials },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      tab: 'sqs',
      loading: false,
      error: null,
      showCreateQueueDialog: false,
      showCreateTopicDialog: false,
      showDeleteQueueDialog: false,
      showDeleteTopicDialog: false,
      showRefreshCredentialsDialog: false,
      queueToDelete: null,
      topicToDelete: null,
    };
  },
  async created() {
    await this.loadData();
  },
  computed: {
    serverInfo() {
      const servers = this.$store.state.sqlite.awsServers || [];
      return servers.find((s) => s.id === parseInt(this.serverId, 10));
    },
    sqsQueues() {
      return this.$store.getters['aws/getSqsQueues'](this.serverId);
    },
    snsTopics() {
      return this.$store.getters['aws/getSnsTopics'](this.serverId);
    },
  },
  methods: {
    async loadData() {
      this.loading = true;
      this.error = null;
      try {
        await Promise.all([
          this.$store.dispatch('aws/loadSqsQueues', this.serverId),
          this.$store.dispatch('aws/loadSnsTopics', this.serverId),
        ]);
      } catch (error) {
        // Check if this is an authentication error
        if (error.message === 'AWS_AUTH_ERROR') {
          this.showRefreshCredentialsDialog = true;
        } else {
          this.error = 'Failed to load AWS resources. Please check your credentials and try again.';
        }
      } finally {
        this.loading = false;
      }
    },
    async handleCredentialsRefreshed() {
      this.showRefreshCredentialsDialog = false;
      // Retry loading data with new credentials
      await this.loadData();
    },
    handleQueueCreated() {
      this.showCreateQueueDialog = false;
      this.$store.dispatch('aws/loadSqsQueues', this.serverId);
    },
    handleTopicCreated() {
      this.showCreateTopicDialog = false;
      this.$store.dispatch('aws/loadSnsTopics', this.serverId);
    },
    confirmDeleteQueue(queue) {
      this.queueToDelete = queue;
      this.showDeleteQueueDialog = true;
    },
    async deleteQueue() {
      if (this.queueToDelete) {
        try {
          await this.$store.dispatch('aws/deleteQueue', {
            serverId: this.serverId,
            queueUrl: this.queueToDelete.queueUrl,
          });
        } catch (error) {
          // Error handled by store
        }
      }
      this.showDeleteQueueDialog = false;
      this.queueToDelete = null;
    },
    confirmDeleteTopic(topic) {
      this.topicToDelete = topic;
      this.showDeleteTopicDialog = true;
    },
    async deleteTopic() {
      if (this.topicToDelete) {
        try {
          await this.$store.dispatch('aws/deleteTopic', {
            serverId: this.serverId,
            topicArn: this.topicToDelete.topicArn,
          });
        } catch (error) {
          // Error handled by store
        }
      }
      this.showDeleteTopicDialog = false;
      this.topicToDelete = null;
    },
  },
};
</script>

<style scoped>
.delete-icon {
  color: blue;
  cursor: pointer;
}

.detail-link {
  color: blue;
  cursor: pointer;
  text-decoration: underline;
}

.text-truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
