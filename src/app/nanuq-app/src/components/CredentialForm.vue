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

      <!-- Info Alert for Optional Credentials -->
      <v-alert
        v-if="serverType === 'Kafka' && !hasExistingCredentials"
        type="info"
        variant="tonal"
        density="compact"
        class="mb-3"
      >
        <template v-slot:prepend>
          <v-icon>mdi-information</v-icon>
        </template>
        Credentials are optional for Kafka. If your broker doesn't require authentication, leave these fields empty and close this dialog.
      </v-alert>

      <!-- Username/Access Key Field -->
      <v-text-field
        v-model="username"
        :label="getUsernameLabel()"
        prepend-icon="mdi-account"
        :rules="(serverType === 'Redis' || serverType === 'Kafka') ? [] : [rules.required]"
        clearable
        :hint="serverType === 'AWS' ? 'Your AWS Access Key ID' : ''"
        :persistent-hint="serverType === 'AWS'"
      />

      <!-- Password/Secret Key Field -->
      <v-text-field
        v-model="password"
        :label="getPasswordLabel()"
        prepend-icon="mdi-key"
        :append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
        :type="showPassword ? 'text' : 'password'"
        :rules="getPasswordRules()"
        @click:append="showPassword = !showPassword"
        clearable
        :hint="serverType === 'AWS' ? 'Your AWS Secret Access Key' : ''"
        :persistent-hint="serverType === 'AWS'"
      />

      <!-- Session Token Field (AWS Only - for MFA/Temporary Credentials) -->
      <v-text-field
        v-if="serverType === 'AWS'"
        v-model="sessionToken"
        label="Session Token (optional - for MFA/temporary credentials)"
        prepend-icon="mdi-clock-outline"
        :append-icon="showSessionToken ? 'mdi-eye' : 'mdi-eye-off'"
        :type="showSessionToken ? 'text' : 'password'"
        @click:append="showSessionToken = !showSessionToken"
        clearable
        hint="Required for MFA-enabled accounts. Generate from AWS Console > Security Credentials > Create Access Key > CLI."
        persistent-hint
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
      validator: (value) => ['Kafka', 'Redis', 'RabbitMQ', 'AWS', 'Azure'].includes(value),
    },
  },
  data() {
    return {
      username: '',
      password: '',
      sessionToken: '',
      showPassword: false,
      showSessionToken: false,
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
      // Redis doesn't require username, Kafka can be optional
      if (this.serverType === 'Redis') {
        return !!this.password;
      }
      if (this.serverType === 'Kafka') {
        // For Kafka, allow testing if both are provided
        return !!this.username && !!this.password;
      }
      return !!this.username && !!this.password;
    },
    isSaveable() {
      if (this.hasExistingCredentials) {
        // For update, allow if either field is filled
        return !!this.username || !!this.password;
      }
      // For new credentials
      if (this.serverType === 'Redis') {
        // Redis only requires password
        return !!this.password;
      }
      if (this.serverType === 'Kafka') {
        // Kafka credentials are optional, but if adding, need both username and password
        // Don't allow saving empty credentials
        return !!this.username && !!this.password;
      }
      // RabbitMQ and AWS require both username and password
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
    getUsernameLabel() {
      if (this.serverType === 'AWS') {
        return 'Access Key ID';
      }
      if (this.serverType === 'Azure') {
        return 'Connection String Name (optional)';
      }
      if (this.serverType === 'Redis') {
        return 'Username (optional for Redis)';
      }
      if (this.serverType === 'Kafka') {
        return 'Username (optional)';
      }
      return 'Username';
    },
    getPasswordLabel() {
      if (this.serverType === 'AWS') {
        if (this.hasExistingCredentials) {
          return 'Secret Access Key (leave blank to keep current)';
        }
        return 'Secret Access Key';
      }
      if (this.serverType === 'Azure') {
        if (this.hasExistingCredentials) {
          return 'Connection String (leave blank to keep current)';
        }
        return 'Connection String';
      }
      if (this.hasExistingCredentials) {
        return 'Password (leave blank to keep current)';
      }
      if (this.serverType === 'Kafka') {
        return 'Password (optional)';
      }
      return 'Password';
    },
    getPasswordRules() {
      if (this.hasExistingCredentials) {
        return [];
      }
      // For Kafka and Redis, password is optional
      if (this.serverType === 'Kafka' || this.serverType === 'Redis') {
        return [];
      }
      return [this.rules.required];
    },
    async loadCredentialMetadata() {
      try {
        await this.$store.dispatch('credentials/fetchCredentialMetadata', {
          serverType: this.serverType,
          serverId: this.serverId,
        });
      } catch (error) {
        // 404 is expected when no credentials exist - no action needed
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
          sessionToken: this.sessionToken || undefined,
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
            sessionToken: this.sessionToken || undefined,
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
            sessionToken: this.sessionToken || undefined,
          });
          this.$emit('saved', { action: 'created' });
        }

        // Clear password and session token fields after successful save
        this.password = '';
        this.sessionToken = '';
        this.testResult = null;
      } catch (error) {
        // Error is already handled by the store
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
        this.sessionToken = '';
        this.testResult = null;
        this.$emit('deleted');
      } catch (error) {
        // Error is already handled by the store
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
