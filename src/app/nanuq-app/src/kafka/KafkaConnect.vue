<template >
  <div class="row">
    <span class="mt-4">Topics in "{{this.serverName}}" </span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active"></li>
  </ol>
    <div class="card mb-4">
    <div class="datatable-wrapper datatable-loading no-footer">
      <div class="datatable-container">
      <table id="servers-table" class="datatable-table">
        <thead>
            <tr>
              <th>Topic Name</th>
              <th>Number of Partitions</th>
              <th>View topics</th>
              <th>Add topic</th>
            </tr>
        </thead>
        <tbody>
          <tr v-for="(topic, index) in availableTopics"
           :key="index">
           <td>{{ topic.topicName }}</td>
           <td>{{ topic.numberOfPartitions }}</td>
           <td>
            <span @click="handleShowListTopicModal(topic.topicName)">
              View topics
            </span>
           </td>
           <td></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="datatable-bottom"></div>
    </div>
    </div>
    <v-dialog v-model="showListTopicsModal" width="600px" >
      <v-card
      prepend-icon="mdi-update"
    >
    <TopicDetails :serverName="serverName" :topicName="selectedTopicName"
          @showModal="show => showModal(show)">

      </TopicDetails>
    </v-card>
    </v-dialog>
  </div>
</template>
<script>
import TopicDetails from './TopicDetails.vue';

export default {
  name: 'KafkaConnect',
  components: { TopicDetails },
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
    // selectedTopicName: {
    //   type: String,
    //   required: false,
    // },
  },
  data() {
    return {
      showListTopicsModal: false,
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
  },
};
</script>
