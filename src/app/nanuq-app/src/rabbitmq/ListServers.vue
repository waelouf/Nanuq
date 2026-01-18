<template>
  <div class="row">
    <span class="mt-4">RabbitMQ servers</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active" />
    </ol>

    <!-- Environment Filter -->
    <div class="mb-3">
      <v-select
        v-model="selectedEnvironment"
        label="Filter by Environment"
        :items="environmentOptions"
        prepend-icon="mdi-filter"
        variant="outlined"
        density="compact"
        style="max-width: 300px;"
      />
    </div>

    <div class="card mb-4">
      <div class="datatable-wrapper datatable-loading no-footer">
        <div class="datatable-container">
          <table id="servers-table" class="datatable-table">
            <thead>
              <tr>
                <th>Id</th>
                <th>Alias</th>
                <th>Server</th>
                <th>Environment</th>
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
                <td>{{ server.serverUrl }}</td>
                <td>
                  <v-chip
                    :color="getEnvironmentColor(server.environment)"
                    size="small"
                    variant="flat"
                  >
                    {{ server.environment || 'Development' }}
                  </v-chip>
                </td>
                <td>
                  <v-icon
                    v-if="hasCredentials('RabbitMQ', server.id)"
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
                      name: 'ManageRabbitMQ',
                      params: {
                        serverUrl: server.serverUrl,
                      },
                    }">
                    Manage
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
        <AddServer
          @showModal="show => {
            showAddServerDialog = show;
            reloadServers();
          }" />
      </v-card>

    </v-dialog>
  </div>

</template>
<script>
import AddServer from './AddServer.vue';

export default ({
  name: 'ListServers',
  components: { AddServer },
  async created() {
    await this.$store.dispatch('sqlite/loadRabbitMQServers');
    // Load credential metadata for all servers
    const servers = this.$store.state.sqlite.rabbitMQServers;
    servers.forEach((server) => {
      this.$store.dispatch('credentials/fetchCredentialMetadata', {
        serverType: 'RabbitMQ',
        serverId: server.id,
      }).catch(() => {
        // Ignore 404 errors for servers without credentials
      });
    });
  },
  computed: {
    availableServers() {
      const servers = this.$store.state.sqlite.rabbitMQServers || [];
      if (this.selectedEnvironment === 'All') {
        return servers;
      }
      return servers.filter((server) => server.environment === this.selectedEnvironment);
    },
    environmentOptions() {
      return ['All', 'Development', 'Staging', 'Production'];
    },
  },
  data() {
    return {
      showAddServerDialog: false,
      selectedEnvironment: 'All',
    };
  },
  methods: {
    hasCredentials(serverType, serverId) {
      return this.$store.getters['credentials/hasCredentials'](serverType, serverId);
    },
    getEnvironmentColor(environment) {
      const colors = {
        Development: 'blue',
        Staging: 'orange',
        Production: 'red',
      };
      return colors[environment] || 'blue';
    },
    addServer() {
      this.showAddServerDialog = true;
    },
    reloadServers() {
      this.$store.dispatch('sqlite/loadRabbitMQServers');
    },
    async deleteServer(id) {
      await this.$store.dispatch('sqlite/deleteRabbitMQServer', id);
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
