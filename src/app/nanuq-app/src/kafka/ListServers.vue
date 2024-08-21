<template >
<div>
  <div v-for="(server, index) in availableServers" :key="index"
  @click="connectToServer(server.id)">
    {{ server.alias }} :
    {{ server.bootstrapServer }}
  </div>
</div>
</template>
<script>

export default ({
  name: 'ListServers',
  created() {
    this.$store.dispatch('sqlite/loadKafkaServers');
  },
  computed: {
    availableServers() {
      return this.$store.state.sqlite.kafkaServers;
    },
  },
  methods: {
    connectToServer(index) {
      this.$router.push({
        name: 'KafkaConnect',
        params: {
          id: index,
        },
      });
    },
  },
});
</script>
