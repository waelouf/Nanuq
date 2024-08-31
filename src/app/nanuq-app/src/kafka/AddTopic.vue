<template>
    <v-sheet class="mx-auto" width="300">
        <v-form fast-fail @submit.prevent>
          <v-text-field
            v-model="bootstrapServer"
            label="Bootstrap Server"
          ></v-text-field>

          <v-text-field
            v-model="topicName"
            label="Topic Name"
          ></v-text-field>

          <v-text-field
            v-model="numberOfPartitions"
            label="Number of partitions"
          ></v-text-field>

          <v-text-field
            v-model="replicationFactor"
            label="Replication factor"
          ></v-text-field>

          <v-btn class="mt-2" @click="addKafkaTopic" type="submit" block>Save</v-btn>
          <br />
        </v-form>
      </v-sheet>
</template>
<script>
export default {
  name: 'AddTopic',
  mounted() {
    this.bootstrapServer = this.serverName;
  },
  props: {
    serverName: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      bootstrapServer: '',
      topicName: '',
      numberOfPartitions: 2,
      replicationFactor: 1,
    };
  },
  methods: {
    async addKafkaTopic() {
      const kafkaTopic =
            {
              bootstrapServers: this.bootstrapServer,
              topicName: this.topicName,
              numberOfPartitions: this.numberOfPartitions,
              replicationFactor: this.replicationFactor,
            };
      await this.$store.dispatch('kafka/addKafkaTopic', kafkaTopic);
      this.$emit('showAddTopicModal', false);
    },
  },
};
</script>
