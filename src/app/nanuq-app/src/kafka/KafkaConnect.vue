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
              <th></th>
            </tr>
        </thead>
        <tbody>
          <tr v-for="(topic, index) in availableTopics"
           :key="index">
           <td>{{ topic.topicName }}</td>
           <td>{{ topic.numberOfPartitions }}</td>
           <td></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="datatable-bottom"></div>
    </div>
    </div>
  </div>
</template>
<script>

export default {
  name: 'KafkaConnect',
  created() {
    this.$store.dispatch('kafka/loadKafkaTopics', this.serverName);
    console.log(this.serverName);
    console.log(this.$store.state.kafka.kafkaTopics);
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

    };
  },
};
</script>
