<template >
<div class="row">
  <span class="mt-4">Kafka servers</span>
  <ol class="breadcrumb mb-4">
    <li class="breadcrumb-item active"></li>
</ol>
  <div class="card mb-4">
  <div class="datatable-wrapper datatable-loading no-footer">
    <div class="datatable-container">
    <table id="servers-table" class="datatable-table">
      <thead>
          <tr>
            <th>Id</th>
            <th>alias</th>
            <th>Server</th>
            <th></th>
          </tr>
      </thead>
      <tbody>
        <tr v-for="(server, index) in availableServers"
         :key="index">
         <td>{{ server.id }}</td>
         <td>{{ server.alias }}</td>
         <td>{{ server.bootstrapServer }}</td>
         <td>
          <router-link :to="{
            name: 'KafkaConnect',
            params: {
              serverName: server.bootstrapServer,
            },
          }" >
          Topics
          </router-link>
         </td>
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
  methods: { },
});
</script>
