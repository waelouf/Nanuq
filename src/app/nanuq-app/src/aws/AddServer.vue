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
          <v-select
            v-model="region"
            label="AWS Region"
            prepend-icon="mdi-earth"
            :items="regions"
            :rules="[rules.required]"
          />

          <v-text-field
            v-model="alias"
            label="Alias"
            prepend-icon="mdi-tag"
            :rules="[rules.required]"
          />

          <v-select
            v-model="environment"
            label="Environment"
            prepend-icon="mdi-server-network"
            :items="environments"
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
              v-if="!savedServerId"
              color="primary"
              @click="saveServer"
              type="submit"
              :disabled="!region || !alias || !environment"
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
          serverType="AWS"
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
  name: 'AddServer',
  components: {
    CredentialForm,
  },
  data() {
    return {
      region: 'us-east-1',
      alias: '',
      environment: 'Development',
      regions: [
        'us-east-1',
        'us-east-2',
        'us-west-1',
        'us-west-2',
        'eu-west-1',
        'eu-west-2',
        'eu-west-3',
        'eu-central-1',
        'ap-southeast-1',
        'ap-southeast-2',
        'ap-northeast-1',
        'ap-northeast-2',
        'ap-south-1',
        'sa-east-1',
        'ca-central-1',
      ],
      environments: ['Development', 'Staging', 'Production'],
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
        'AWS',
        this.savedServerId
      );
    },
  },
  methods: {
    async saveServer() {
      const serverDetails = {
        region: this.region,
        alias: this.alias,
        environment: this.environment,
        serviceType: 'SQS',
      };

      try {
        // Save the server and get the ID directly
        const serverId = await this.$store.dispatch('sqlite/addAwsServer', serverDetails);

        if (serverId) {
          this.savedServerId = serverId;
          // Switch to credentials tab
          this.tab = 'credentials';
        }
      } catch (error) {
        // Error is already handled by the store
      }
    },
    handleCredentialSaved() {
      // Credentials saved notification is handled by the store
      // Auto-close dialog after a short delay to show the success message
      setTimeout(() => {
        this.closeDialog();
      }, 1500);
    },
    handleCredentialDeleted() {
      // Credentials deleted notification is handled by the store
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
