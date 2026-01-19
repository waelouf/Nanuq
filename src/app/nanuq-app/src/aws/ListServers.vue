<template>
  <div class="row">
    <span class="mt-4">AWS Servers</span>
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

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <v-progress-circular
        indeterminate
        color="primary"
        size="64"
      />
    </div>

    <!-- Empty State -->
    <div v-else-if="availableServers.length === 0" class="text-center py-5">
      <v-icon size="100" color="grey-lighten-1">mdi-aws</v-icon>
      <h3 class="mt-4 mb-2">No AWS Servers Configured</h3>
      <p class="text-muted">Add your first AWS server to get started with SQS and SNS management.</p>
      <v-btn
        color="primary"
        class="mt-4"
        @click="addServer"
        prepend-icon="mdi-plus"
      >
        Add Your First Server
      </v-btn>
    </div>

    <!-- Server Cards -->
    <div v-else class="row">
      <div
        v-for="server in availableServers"
        :key="server.id"
        class="col-12 col-md-6 col-lg-4 mb-4"
      >
        <v-card
          hover
          @click="manageServer(server.id)"
          class="server-card"
        >
          <v-card-title class="d-flex justify-space-between align-center">
            <div>
              <v-icon class="mr-2">mdi-aws</v-icon>
              {{ server.alias }}
            </div>
            <v-icon
              v-if="hasCredentials('AWS', server.id)"
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
          </v-card-title>

          <v-card-text>
            <div class="mb-2">
              <strong>Region:</strong> {{ server.region }}
            </div>
            <div class="mb-2">
              <strong>Service:</strong> {{ server.serviceType || 'SQS/SNS' }}
            </div>
            <div>
              <v-chip
                :color="getEnvironmentColor(server.environment)"
                size="small"
                variant="flat"
              >
                {{ server.environment || 'Development' }}
              </v-chip>
            </div>
          </v-card-text>

          <v-card-actions>
            <v-btn
              variant="text"
              color="primary"
              @click.stop="manageServer(server.id)"
            >
              Manage
            </v-btn>
            <v-spacer />
            <v-btn
              variant="text"
              color="error"
              @click.stop="confirmDelete(server)"
              prepend-icon="mdi-delete"
            >
              Delete
            </v-btn>
          </v-card-actions>
        </v-card>
      </div>
    </div>

    <v-btn
      v-if="availableServers.length > 0"
      class="mt-2"
      @click="addServer"
      type="submit"
      block
      prepend-icon="mdi-plus"
    >
      Add Server
    </v-btn>

    <!-- Add Server Dialog -->
    <v-dialog v-model="showAddServerDialog" width="600px">
      <v-card prepend-icon="mdi-aws">
        <AddServer
          @showModal="show => {
            showAddServerDialog = show;
            reloadServers();
          }"
        />
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5">Confirm Deletion</v-card-title>
        <v-card-text>
          Are you sure you want to delete the AWS server "{{ serverToDelete?.alias }}"?
          This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="grey" variant="text" @click="showDeleteDialog = false">
            Cancel
          </v-btn>
          <v-btn color="error" variant="text" @click="deleteServer">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
import AddServer from './AddServer.vue';

export default {
  name: 'ListServers',
  components: { AddServer },
  async created() {
    this.loading = true;
    await this.$store.dispatch('sqlite/loadAwsServers');
    // Load credential metadata for all servers
    const servers = this.$store.state.sqlite.awsServers;
    servers.forEach((server) => {
      this.$store.dispatch('credentials/fetchCredentialMetadata', {
        serverType: 'AWS',
        serverId: server.id,
      }).catch(() => {
        // Ignore 404 errors for servers without credentials
      });
    });
    this.loading = false;
  },
  computed: {
    availableServers() {
      const servers = this.$store.state.sqlite.awsServers || [];
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
      showDeleteDialog: false,
      selectedEnvironment: 'All',
      serverToDelete: null,
      loading: false,
    };
  },
  methods: {
    hasCredentials(serverType, serverId) {
      return this.$store.getters['credentials/hasCredentials'](serverType, serverId);
    },
    getEnvironmentColor(environment) {
      const colors = {
        Development: 'success',
        Staging: 'warning',
        Production: 'error',
      };
      return colors[environment] || 'success';
    },
    addServer() {
      this.showAddServerDialog = true;
    },
    manageServer(serverId) {
      this.$router.push({ name: 'ManageAWS', params: { serverId } });
    },
    reloadServers() {
      this.$store.dispatch('sqlite/loadAwsServers');
    },
    confirmDelete(server) {
      this.serverToDelete = server;
      this.showDeleteDialog = true;
    },
    async deleteServer() {
      if (this.serverToDelete) {
        await this.$store.dispatch('sqlite/deleteAwsServer', this.serverToDelete.id);
        this.reloadServers();
      }
      this.showDeleteDialog = false;
      this.serverToDelete = null;
    },
  },
};
</script>

<style scoped>
.server-card {
  cursor: pointer;
  transition: transform 0.2s;
}

.server-card:hover {
  transform: translateY(-4px);
}
</style>
