<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="text-h5 mb-4">Add Azure Server</v-card-title>

    <v-tabs v-model="tab" color="primary">
      <v-tab value="details">Server Details</v-tab>
      <v-tab value="credentials" :disabled="!serverId">Credentials</v-tab>
    </v-tabs>

    <v-tabs-window v-model="tab" class="mt-4">
      <!-- Server Details Tab -->
      <v-tabs-window-item value="details">
        <v-form fast-fail @submit.prevent="saveServer">
          <v-text-field
            v-model="alias"
            label="Alias"
            prepend-icon="mdi-label"
            :rules="[rules.required]"
            hint="Friendly name for this server"
            persistent-hint
          />

          <v-text-field
            v-model="namespace"
            label="Service Bus Namespace"
            prepend-icon="mdi-microsoft-azure"
            :rules="[rules.required]"
            hint="e.g., myapp.servicebus.windows.net"
            persistent-hint
          />

          <v-select
            v-model="region"
            label="Azure Region"
            prepend-icon="mdi-earth"
            :items="regions"
            :rules="[rules.required]"
            hint="Select the Azure region"
            persistent-hint
          />

          <v-select
            v-model="environment"
            label="Environment"
            prepend-icon="mdi-tag"
            :items="environments"
            :rules="[rules.required]"
            hint="Select the environment type"
            persistent-hint
          />

          <v-alert v-if="error" type="error" variant="tonal" class="mt-3">
            {{ error }}
          </v-alert>

          <div class="d-flex justify-end gap-2 mt-4">
            <v-btn
              color="secondary"
              variant="outlined"
              @click="close"
            >
              Cancel
            </v-btn>
            <v-btn
              v-if="!serverId"
              color="primary"
              type="submit"
              :disabled="!alias || !namespace || !region || loading"
              :loading="loading"
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
          v-if="serverId"
          :server-id="serverId"
          server-type="Azure"
          @saved="handleCredentialsSaved"
          @deleted="handleCredentialsDeleted"
        />
      </v-tabs-window-item>
    </v-tabs-window>
  </v-sheet>
</template>

<script>
import CredentialForm from '@/components/CredentialForm.vue';

export default {
  name: 'AddAzureServer',
  components: {
    CredentialForm,
  },
  data() {
    return {
      tab: 'details',
      serverId: null,
      alias: '',
      namespace: '',
      region: 'East US',
      environment: 'Development',
      loading: false,
      error: null,
      regions: [
        'East US',
        'East US 2',
        'West US',
        'West US 2',
        'West US 3',
        'Central US',
        'North Central US',
        'South Central US',
        'West Central US',
        'Canada Central',
        'Canada East',
        'Brazil South',
        'UK South',
        'UK West',
        'West Europe',
        'North Europe',
        'France Central',
        'Germany West Central',
        'Switzerland North',
        'Norway East',
        'Southeast Asia',
        'East Asia',
        'Australia East',
        'Australia Southeast',
        'Central India',
        'South India',
        'Japan East',
        'Japan West',
        'Korea Central',
        'South Africa North',
      ],
      environments: ['Development', 'Staging', 'Production'],
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  methods: {
    async saveServer() {
      this.loading = true;
      this.error = null;

      try {
        const serverDetails = {
          alias: this.alias,
          namespace: this.namespace,
          region: this.region,
          environment: this.environment,
          serviceType: 'ServiceBus',
        };

        const response = await this.$store.dispatch('sqlite/addAzureServer', serverDetails);
        this.serverId = response.id;

        // Switch to credentials tab
        this.tab = 'credentials';
      } catch (error) {
        this.error = 'Failed to save server. Please try again.';
      } finally {
        this.loading = false;
      }
    },
    handleCredentialsSaved() {
      // Auto-close dialog after credentials saved
      setTimeout(() => {
        this.close();
      }, 1500);
    },
    handleCredentialsDeleted() {
      // Optionally handle credential deletion
    },
    close() {
      this.$emit('close');
    },
  },
};
</script>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
