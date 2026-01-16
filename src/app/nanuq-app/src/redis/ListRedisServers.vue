<template>
  <div class="row">
    <span class="mt-4">Redis servers</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active" />
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
                <th>Auth</th>
                <th />
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(server, index) in availableServers"
                :key="index">
                <td>{{ server.id }}</td>
                <td>{{ server.alias }}</td>
                <td>{{ server.serverUrl }}
                  <a @click="showServerDetails(server.serverUrl)">
                    <i class="fa-solid fa-circle-info" />
                  </a></td>
                <td>
                  <v-icon
                    v-if="hasCredentials('Redis', server.id)"
                    color="success"
                    size="small"
                    title="Credentials configured"
                  >
                    mdi-shield-lock
                  </v-icon>
                  <v-icon
                    v-else
                    color="grey"
                    size="small"
                    title="No credentials"
                  >
                    mdi-shield-off
                  </v-icon>
                </td>
                <td>
                  <router-link
                    :to="{
                      name: 'ManageServer',
                      params: {
                        serverUrl: server.serverUrl,
                      },
                    }">
                    Manage server
                  </router-link>
                </td>
                <td>
                  <a @click="deleteServer(server.id)" class="delete-icon">
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
    <v-btn class="mt-2" @click="addServer" type="submit" block>Add Server</v-btn>
    <v-dialog v-model="showAddServerDialog" width="600px">
      <v-card
        prepend-icon="mdi-update">
        <AddRedisServer
          @showModal="show => {
            showAddServerDialog = show;
            reloadServers();
          }" />
      </v-card>

    </v-dialog>
    <v-dialog v-model="showServerDetailsDialog">
      <v-card
        prepend-icon="mdi-update">
        <ViewServerFullDetails
          :serverUrl="selectedServerUrl"
          @showServerDetails="show => { showServerDetailsDialog = show }" />
      </v-card>

    </v-dialog>
  </div>

</template>
<script>
import AddRedisServer from './AddRedisServer.vue';
import ViewServerFullDetails from './ViewServerFullDetails.vue';

export default ({
  name: 'ListServers',
  components: { AddRedisServer, ViewServerFullDetails },
  async created() {
    await this.$store.dispatch('sqlite/loadRedisServers');
    // Load credential metadata for all servers
    const servers = this.$store.state.sqlite.redisServers;
    servers.forEach((server) => {
      this.$store.dispatch('credentials/fetchCredentialMetadata', {
        serverType: 'Redis',
        serverId: server.id,
      }).catch(() => {
        // Ignore 404 errors for servers without credentials
      });
    });
  },
  computed: {
    availableServers() {
      return this.$store.state.sqlite.redisServers;
    },
  },
  data() {
    return {
      showAddServerDialog: false,
      showServerDetailsDialog: false,
      selectedServerUrl: '',
    };
  },
  methods: {
    hasCredentials(serverType, serverId) {
      return this.$store.getters['credentials/hasCredentials'](serverType, serverId);
    },
    addServer() {
      this.showAddServerDialog = true;
    },
    async reloadServers() {
      await this.$store.dispatch('sqlite/loadRedisServers');
      // Load credential metadata for all servers
      const servers = this.$store.state.sqlite.redisServers;
      servers.forEach((server) => {
        this.$store.dispatch('credentials/fetchCredentialMetadata', {
          serverType: 'Redis',
          serverId: server.id,
        }).catch(() => {
          // Ignore 404 errors for servers without credentials
        });
      });
    },
    async deleteServer(id) {
      await this.$store.dispatch('sqlite/deleteRedisServer', id);
      await this.reloadServers();
    },
    async showServerDetails(serverUrl) {
      await this.$store.dispatch('redis/getServerDetails', serverUrl);
      this.selectedServerUrl = serverUrl;
      this.showServerDetailsDialog = true;
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
