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
            <table id="topics-table" class="datatable-table">
              <thead>
                <tr>
                  <th>Topic Name</th>
                  <th>Subscriptions</th>
                  <th>Max Size</th>
                  <th>Status</th>
                  <th />
                  <th />
                </tr>
              </thead>
              <tbody>
                <tr v-if="topics.length === 0">
                  <td colspan="6" class="text-center text-muted py-4">
                    No topics found. Create your first topic to get started.
                  </td>
                </tr>
                <tr
                  v-for="(topic, index) in topics"
                  :key="index"
                >
                  <td>{{ topic.name }}</td>
                  <td>
                    <v-chip size="small" color="success">
                      {{ topic.subscriptionCount }}
                    </v-chip>
                  </td>
                  <td>{{ topic.maxSizeInMegabytes }} MB</td>
                  <td>
                    <v-chip size="small" :color="topic.status === 'Active' ? 'success' : 'warning'">
                      {{ topic.status }}
                    </v-chip>
                  </td>
                  <td>
                    <a @click="viewTopicDetails(topic)" class="detail-link">
                      Details
                    </a>
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
    </div>

    <!-- Create Topic Dialog -->
    <v-dialog v-model="showCreateTopicDialog" width="600px">
      <v-card prepend-icon="mdi-bullhorn-outline">
        <CreateTopic :server-id="serverId" @close="handleTopicCreated" />
      </v-card>
    </v-dialog>

    <!-- Topic Details Dialog -->
    <v-dialog v-model="showTopicDetailsDialog" width="900px">
      <v-card prepend-icon="mdi-information">
        <TopicDetails
          :server-id="serverId"
          :topic="selectedTopic"
          @close="showTopicDetailsDialog = false"
        />
      </v-card>
    </v-dialog>

    <!-- Delete Topic Confirmation Dialog -->
    <v-dialog v-model="showDeleteTopicDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Topic Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the topic "{{ topicToDelete?.name }}"?
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
  </div>
</template>

<script>
import CreateTopic from './CreateTopic.vue';
import TopicDetails from './TopicDetails.vue';

export default {
  name: 'ManageTopics',
  components: { CreateTopic, TopicDetails },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      loading: false,
      showCreateTopicDialog: false,
      showTopicDetailsDialog: false,
      showDeleteTopicDialog: false,
      topicToDelete: null,
      selectedTopic: null,
    };
  },
  async created() {
    await this.loadTopics();
  },
  computed: {
    topics() {
      return this.$store.state.azure.topics || [];
    },
  },
  methods: {
    async loadTopics() {
      this.loading = true;
      try {
        await this.$store.dispatch('azure/loadTopics', this.serverId);
      } catch (error) {
        // Error handled by store
      } finally {
        this.loading = false;
      }
    },
    viewTopicDetails(topic) {
      this.selectedTopic = topic;
      this.showTopicDetailsDialog = true;
    },
    handleTopicCreated() {
      this.showCreateTopicDialog = false;
      this.loadTopics();
    },
    confirmDeleteTopic(topic) {
      this.topicToDelete = topic;
      this.showDeleteTopicDialog = true;
    },
    async deleteTopic() {
      if (this.topicToDelete) {
        try {
          await this.$store.dispatch('azure/deleteTopic', {
            serverId: this.serverId,
            topicName: this.topicToDelete.name,
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
</style>
