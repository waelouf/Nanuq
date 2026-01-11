<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-tabs v-model="tab" class="mb-4">
      <v-tab value="server">
        <v-icon start>mdi-server</v-icon>
        Server Details
      </v-tab>
      <v-tab value="credentials" :disabled="!savedServerId">
        <v-icon start>mdi-lock</v-icon>
        Credentials
        <v-chip
          v-if="hasCredentials"
          class="ml-2"
          color="success"
          size="x-small"
        >
          <v-icon size="x-small">mdi-check</v-icon>
        </v-chip>
      </v-tab>
    </v-tabs>

    <v-tabs-window v-model="tab">
      <!-- Server Details Tab -->
      <v-tabs-window-item value="server">
        <v-form fast-fail @submit.prevent>
          <v-text-field
            v-model="serverUrl"
            label="Server URL"
            prepend-icon="mdi-server-network"
            :rules="[rules.required]"
          />

          <v-text-field
            v-model="alias"
            label="Alias"
            prepend-icon="mdi-tag"
            :rules="[rules.required]"
          />

          <v-alert
            v-if="savedServerId"
            type="success"
            variant="tonal"
            class="mt-3"
          >
            Server saved successfully! You can now add credentials in the Credentials tab.
          </v-alert>

          <div class="d-flex justify-end gap-2 mt-4">
            <v-btn
              color="secondary"
              variant="outlined"
              @click="closeDialog"
            >
              {{ savedServerId ? 'Close' : 'Cancel' }}
            </v-btn>
            <v-btn
              color="primary"
              @click="saveServer"
              type="submit"
              :disabled="!serverUrl || !alias"
            >
              <v-icon start>mdi-content-save</v-icon>
              Save Server
            </v-btn>
          </div>
        </v-form>
      </v-tabs-window-item>

      <!-- Credentials Tab -->
      <v-tabs-window-item value="credentials">
        <CredentialForm
          v-if="savedServerId"
          :serverId="savedServerId"
          serverType="Redis"
          @saved="handleCredentialSaved"
          @deleted="handleCredentialDeleted"
        />
      </v-tabs-window-item>
    </v-tabs-window>
  </v-sheet>
</template>

<script>
import CredentialForm from '@/components/CredentialForm.vue';

export default {
  name: 'AddRedisServer',
  components: {
    CredentialForm,
  },
  data() {
    return {
      serverUrl: '',
      alias: '',
      tab: 'server',
      savedServerId: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  computed: {
    hasCredentials() {
      if (!this.savedServerId) return false;
      return this.$store.getters['credentials/hasCredentials'](
        'Redis',
        this.savedServerId
      );
    },
  },
  methods: {
    async saveServer() {
      const serverDetails = {
        serverUrl: this.serverUrl,
        alias: this.alias,
      };

      try {
        // Dispatch action to save server
        await this.$store.dispatch('sqlite/addRedisServer', serverDetails);

        // Reload servers to get the ID
        await this.$store.dispatch('sqlite/loadRedisServers');
        const servers = this.$store.state.sqlite.redisServers;
        const savedServer = servers.find(
          (s) => s.serverUrl === this.serverUrl && s.alias === this.alias
        );

        if (savedServer) {
          this.savedServerId = savedServer.id;
          // Switch to credentials tab
          this.tab = 'credentials';
        }
      } catch (error) {
        console.error('Error saving server:', error);
      }
    },
    handleCredentialSaved() {
      console.log('Credentials saved successfully');
    },
    handleCredentialDeleted() {
      console.log('Credentials deleted successfully');
    },
    closeDialog() {
      this.$emit('showModal', false);
    },
  },
};
</script>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
