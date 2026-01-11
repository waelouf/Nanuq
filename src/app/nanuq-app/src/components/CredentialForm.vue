<template>
  <v-sheet class="mx-auto" width="100%">
    <v-form fast-fail @submit.prevent>
      <!-- Credential Header -->
      <div class="d-flex align-center mb-3">
        <v-icon color="primary" class="mr-2">mdi-lock</v-icon>
        <h3>Server Credentials</h3>
        <v-chip
          v-if="hasExistingCredentials"
          class="ml-2"
          color="success"
          size="small"
        >
          <v-icon start>mdi-shield-check</v-icon>
          Encrypted
        </v-chip>
      </div>

      <v-alert
        v-if="testResult"
        :type="testResult.success ? 'success' : 'error'"
        class="mb-3"
        closable
        @click:close="testResult = null"
      >
        {{ testResult.message }}
      </v-alert>

      <!-- Username Field -->
      <v-text-field
        v-model="username"
        label="Username"
        prepend-icon="mdi-account"
        :rules="[rules.required]"
        clearable
      />

      <!-- Password Field -->
      <v-text-field
        v-model="password"
        :label="hasExistingCredentials ? 'Password (leave blank to keep current)' : 'Password'"
        prepend-icon="mdi-key"
        :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
        :type="showPassword ? 'text' : 'password'"
        :rules="hasExistingCredentials ? [] : [rules.required]"
        @click:append="showPassword = !showPassword"
        clearable
      />

      <!-- Action Buttons -->
      <div class="d-flex justify-end gap-2 mt-4">
        <v-btn
          color="secondary"
          variant="outlined"
          @click="handleTestConnection"
          :loading="testing"
          :disabled="!isTestable"
        >
          <v-icon start>mdi-test-tube</v-icon>
          Test Connection
        </v-btn>

        <v-btn
          v-if="hasExistingCredentials"
          color="error"
          variant="outlined"
          @click="handleDelete"
          :loading="deleting"
        >
          <v-icon start>mdi-delete</v-icon>
          Remove Credentials
        </v-btn>

        <v-btn
          color="primary"
          @click="handleSave"
          :loading="saving"
          :disabled="!isSaveable"
        >
          <v-icon start>mdi-content-save</v-icon>
          {{ hasExistingCredentials ? 'Update' : 'Save' }} Credentials
        </v-btn>
      </div>

      <!-- Info Text -->
      <v-alert
        type="info"
        variant="tonal"
        class="mt-4"
        density="compact"
      >
        <template v-slot:prepend>
          <v-icon>mdi-information</v-icon>
        </template>
        Credentials are encrypted using AES-256 before storage. Passwords are never exposed in API responses.
      </v-alert>
    </v-form>
  </v-sheet>
</template>

<script>
export default {
  name: 'CredentialForm',
  props: {
    serverId: {
      type: Number,
      required: true,
    },
    serverType: {
      type: String,
      required: true,
      validator: (value) => ['Kafka', 'Redis', 'RabbitMQ'].includes(value),
    },
  },
  data() {
    return {
      username: '',
      password: '',
      showPassword: false,
      testing: false,
      saving: false,
      deleting: false,
      testResult: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  computed: {
    hasExistingCredentials() {
      return this.$store.getters['credentials/hasCredentials'](
        this.serverType,
        this.serverId
      );
    },
    credentialMetadata() {
      return this.$store.getters['credentials/getMetadata'](
        this.serverType,
        this.serverId
      );
    },
    isTestable() {
      return !!this.username && !!this.password;
    },
    isSaveable() {
      if (this.hasExistingCredentials) {
        // For update, allow if either field is filled
        return !!this.username || !!this.password;
      }
      // For new credentials, both required
      return !!this.username && !!this.password;
    },
  },
  async mounted() {
    // Fetch existing credentials metadata if available
    if (this.serverId) {
      await this.loadCredentialMetadata();
    }
  },
  methods: {
    async loadCredentialMetadata() {
      try {
        await this.$store.dispatch('credentials/fetchCredentialMetadata', {
          serverType: this.serverType,
          serverId: this.serverId,
        });
      } catch (error) {
        // 404 is expected when no credentials exist
        if (error.response?.status !== 404) {
          console.error('Error loading credential metadata:', error);
        }
      }
    },
    async handleTestConnection() {
      if (!this.isTestable) return;

      this.testing = true;
      this.testResult = null;

      try {
        const result = await this.$store.dispatch('credentials/testConnection', {
          serverId: this.serverId,
          serverType: this.serverType,
          username: this.username,
          password: this.password,
        });
        this.testResult = result;
      } catch (error) {
        this.testResult = {
          success: false,
          message: `Connection test failed: ${error.message}`,
        };
      } finally {
        this.testing = false;
      }
    },
    async handleSave() {
      if (!this.isSaveable) return;

      this.saving = true;

      try {
        if (this.hasExistingCredentials) {
          // Update existing credentials
          await this.$store.dispatch('credentials/updateCredentials', {
            credentialId: this.credentialMetadata.id,
            username: this.username || undefined,
            password: this.password || undefined,
            serverId: this.serverId,
            serverType: this.serverType,
          });
          this.$emit('saved', { action: 'updated' });
        } else {
          // Save new credentials
          await this.$store.dispatch('credentials/saveCredentials', {
            serverId: this.serverId,
            serverType: this.serverType,
            username: this.username,
            password: this.password,
          });
          this.$emit('saved', { action: 'created' });
        }

        // Clear password field after successful save
        this.password = '';
        this.testResult = {
          success: true,
          message: 'Credentials saved successfully!',
        };
      } catch (error) {
        this.testResult = {
          success: false,
          message: `Failed to save credentials: ${error.message}`,
        };
      } finally {
        this.saving = false;
      }
    },
    async handleDelete() {
      if (!this.hasExistingCredentials) return;

      this.deleting = true;

      try {
        await this.$store.dispatch('credentials/deleteCredentials', {
          credentialId: this.credentialMetadata.id,
          serverType: this.serverType,
          serverId: this.serverId,
        });

        this.username = '';
        this.password = '';
        this.testResult = {
          success: true,
          message: 'Credentials removed successfully!',
        };
        this.$emit('deleted');
      } catch (error) {
        this.testResult = {
          success: false,
          message: `Failed to delete credentials: ${error.message}`,
        };
      } finally {
        this.deleting = false;
      }
    },
  },
};
</script>

<style scoped>
.gap-2 {
  gap: 8px;
}
</style>
