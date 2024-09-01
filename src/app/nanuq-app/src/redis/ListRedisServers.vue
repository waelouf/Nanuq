<template >
    <div class="row">
      <span class="mt-4">Redis servers</span>
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
                <th></th>
              </tr>
          </thead>
          <tbody>
            <tr v-for="(server, index) in availableServers"
             :key="index">
             <td>{{ server.id }}</td>
             <td>{{ server.alias }}</td>
             <td>{{ server.serverUrl }}</td>
             <td>
                <router-link :to="{
                    name: 'ManageServer',
                    params: {
                        serverUrl: server.serverUrl,
                    },
                  }" >
                  Manage server
                  </router-link>
             </td>
             <td>
              <a @click="deleteServer(server.id)" class="delete-icon">
                <i class="fa-regular fa-trash-can"></i>
              </a>
             </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="datatable-bottom"></div>
      </div>
      </div>
      <v-btn class="mt-2" @click="addServer" type="submit" block>Add Server</v-btn>
      <v-dialog v-model="showAddServerDialog" width="600px" >
        <v-card
          prepend-icon="mdi-update">
        <AddRedisServer @showModal="show => {
            showAddServerDialog = show;
            reloadServers();
          }">

        </AddRedisServer>
      </v-card>

      </v-dialog>
    </div>

</template>
<script>
import AddRedisServer from './AddRedisServer.vue';

export default ({
  name: 'ListServers',
  components: { AddRedisServer },
  created() {
    this.$store.dispatch('sqlite/loadRedisServers');
  },
  computed: {
    availableServers() {
      return this.$store.state.sqlite.redisServers;
    },
  },
  data() {
    return {
      showAddServerDialog: false,
    };
  },
  methods: {
    addServer() {
      this.showAddServerDialog = true;
    },
    reloadServers() {
      this.$store.dispatch('sqlite/loadRedisServers');
    },
    async deleteServer(id) {
      await this.$store.dispatch('sqlite/deleteRedisServer', id);
      this.reloadServers();
    },
  },
});
</script>
    <style>
    .delete-icon{
      color: blue;
      cursor: pointer;
    }
    </style>
