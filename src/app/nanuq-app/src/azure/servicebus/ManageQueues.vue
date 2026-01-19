<template>
  <div>
    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <v-progress-circular
        indeterminate
        color="primary"
        size="64"
      />
    </div>

    <!-- Main Content -->
    <div v-else>
      <div class="card mb-4">
        <div class="datatable-wrapper datatable-loading no-footer">
          <div class="datatable-container">
            <table id="queues-table" class="datatable-table">
              <thead>
                <tr>
                  <th>Queue Name</th>
                  <th>Messages</th>
                  <th>Dead Letter</th>
                  <th>Status</th>
                  <th />
                  <th />
                </tr>
              </thead>
              <tbody>
                <tr v-if="queues.length === 0">
                  <td colspan="6" class="text-center text-muted py-4">
                    No queues found. Create your first queue to get started.
                  </td>
                </tr>
                <tr
                  v-for="(queue, index) in queues"
                  :key="index"
                >
                  <td>{{ queue.name }}</td>
                  <td>
                    <v-chip size="small" color="primary">
                      {{ queue.messageCount }}
                    </v-chip>
                  </td>
                  <td>
                    <v-chip size="small" :color="queue.deadLetterMessageCount > 0 ? 'error' : 'grey'">
                      {{ queue.deadLetterMessageCount }}
                    </v-chip>
                  </td>
                  <td>
                    <v-chip size="small" :color="queue.status === 'Active' ? 'success' : 'warning'">
                      {{ queue.status }}
                    </v-chip>
                  </td>
                  <td>
                    <a @click="viewQueueDetails(queue)" class="detail-link">
                      Details
                    </a>
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
    </div>

    <!-- Create Queue Dialog -->
    <v-dialog v-model="showCreateQueueDialog" width="600px">
      <v-card prepend-icon="mdi-message-processing">
        <CreateQueue :server-id="serverId" @close="handleQueueCreated" />
      </v-card>
    </v-dialog>

    <!-- Queue Details Dialog -->
    <v-dialog v-model="showQueueDetailsDialog" width="900px">
      <v-card prepend-icon="mdi-information">
        <QueueDetails
          :server-id="serverId"
          :queue="selectedQueue"
          @close="showQueueDetailsDialog = false"
        />
      </v-card>
    </v-dialog>

    <!-- Delete Queue Confirmation Dialog -->
    <v-dialog v-model="showDeleteQueueDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Queue Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the queue "{{ queueToDelete?.name }}"?
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
  </div>
</template>

<script>
import CreateQueue from './CreateQueue.vue';
import QueueDetails from './QueueDetails.vue';

export default {
  name: 'ManageQueues',
  components: { CreateQueue, QueueDetails },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      loading: false,
      showCreateQueueDialog: false,
      showQueueDetailsDialog: false,
      showDeleteQueueDialog: false,
      queueToDelete: null,
      selectedQueue: null,
    };
  },
  async created() {
    await this.loadQueues();
  },
  computed: {
    queues() {
      return this.$store.state.azure.queues || [];
    },
  },
  methods: {
    async loadQueues() {
      this.loading = true;
      try {
        await this.$store.dispatch('azure/loadQueues', this.serverId);
      } catch (error) {
        // Error handled by store
      } finally {
        this.loading = false;
      }
    },
    viewQueueDetails(queue) {
      this.selectedQueue = queue;
      this.showQueueDetailsDialog = true;
    },
    handleQueueCreated() {
      this.showCreateQueueDialog = false;
      this.loadQueues();
    },
    confirmDeleteQueue(queue) {
      this.queueToDelete = queue;
      this.showDeleteQueueDialog = true;
    },
    async deleteQueue() {
      if (this.queueToDelete) {
        try {
          await this.$store.dispatch('azure/deleteQueue', {
            serverId: this.serverId,
            queueName: this.queueToDelete.name,
          });
        } catch (error) {
          // Error handled by store
        }
      }
      this.showDeleteQueueDialog = false;
      this.queueToDelete = null;
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
</style>
