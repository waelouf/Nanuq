<template>
  <div class="row">
    <span class="mt-4">Topics in "{{this.serverName}}" </span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active" />
    </ol>
    <div class="card mb-4">
      <div class="datatable-wrapper datatable-loading no-footer">
        <div class="datatable-container">
          <table id="servers-table" class="datatable-table">
            <thead>
              <tr>
                <th>Topic Name</th>
                <th>Number of Partitions</th>
                <th />
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(topic, index) in availableTopics"
                :key="index">
                <td>{{ topic.topicName }}</td>
                <td>{{ topic.numberOfPartitions }}</td>
                <td>
                  <span @click="handleShowListTopicModal(topic.topicName)" class="delete-icon">
                    Topic Details
                  </span>
                </td>
                <td>
                  <a @click="deleteTopic(topic.topicName)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="datatable-bottom" />
      </div>
    </div>
    <v-btn class="mt-2" @click="showAddTopicModalModal(true)" type="submit" block>Add Topic</v-btn>
    <v-dialog v-model="showListTopicsModal" width="600px">
      <v-card
        prepend-icon="mdi-update"
      >
        <TopicDetails
          :serverName="serverName"
          :topicName="selectedTopicName"
          @showModal="show => showModal(show)" />
      </v-card>
    </v-dialog>

    <v-dialog v-model="showAddTopicModal" width="600px">
      <v-card
        prepend-icon="mdi-update"
      >
        <AddTopic
          :serverName="serverName"
          @showAddTopicModal="show => showAddTopicModalModal(show)"
        />
      </v-card>
    </v-dialog>
  </div>
</template>
<script>
import TopicDetails from './TopicDetails.vue';
import AddTopic from './AddTopic.vue';

export default {
  name: 'KafkaConnect',
  components: { TopicDetails, AddTopic },
  created() {
    this.$store.dispatch('kafka/loadKafkaTopics', this.serverName);
  },
  computed: {
    availableTopics() {
      return this.$store.state.kafka.kafkaTopics[this.serverName];
    },
  },
  props: {
    serverName: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      showListTopicsModal: false,
      showAddTopicModal: false,
      selectedTopicName: '',
    };
  },
  methods: {
    handleShowListTopicModal(topicName) {
      this.selectedTopicName = topicName;
      this.showModal(true);
    },
    showModal(isShown) {
      this.showListTopicsModal = isShown;
    },
    showAddTopicModalModal(show) {
      this.showAddTopicModal = show;
      if (!show) {
        this.reloadTopics();
      }
    },
    reloadTopics() {
      this.$store.dispatch('kafka/loadKafkaTopics', this.serverName);
    },
    async deleteTopic(topicName) {
      const topicToDelete = {
        bootstrapServer: this.serverName,
        topicName,
      };
      await this.$store.dispatch('kafka/deleteKafkaTopic', topicToDelete);
      this.reloadTopics();
    },
  },
};
</script>
