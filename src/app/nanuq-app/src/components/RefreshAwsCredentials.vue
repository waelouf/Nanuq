<template>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-icon color="warning" class="mr-2">mdi-alert-circle</v-icon>
      AWS Credentials Expired
    </v-card-title>

    <v-card-text>
      <v-alert type="warning" variant="tonal" class="mb-4">
        Your AWS session token has expired. Please provide fresh credentials to continue.
      </v-alert>

      <v-expansion-panels class="mb-4">
        <v-expansion-panel>
          <v-expansion-panel-title>
            <v-icon start>mdi-information</v-icon>
            How to get temporary credentials with MFA
          </v-expansion-panel-title>
          <v-expansion-panel-text>
            <ol class="pl-4">
              <li>Open your terminal or command prompt</li>
              <li>Run the following command:
                <v-code class="d-block my-2 pa-2 bg-grey-lighten-3">
                  aws sts get-session-token --duration-seconds 43200
                </v-code>
              </li>
              <li>Copy the AccessKeyId, SecretAccessKey, and SessionToken from the output</li>
              <li>Paste them into the fields below</li>
            </ol>
            <v-alert type="info" variant="tonal" density="compact" class="mt-2">
              <strong>Note:</strong> Session tokens expire after 1-12 hours depending on your configuration.
            </v-alert>
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>

      <v-alert
        v-if="testResult"
        :type="testResult.success ? 'success' : 'error'"
        class="mb-3"
        closable
        @click:close="testResult = null"
      >
        {{ testResult.message }}
      </v-alert>

      <!-- Access Key ID Field -->
      <v-text-field
        v-model="accessKeyId"
        label="Access Key ID"
        prepend-icon="mdi-account"
        :rules="[rules.required]"
        clearable
        hint="Your AWS Access Key ID from STS response"
        persistent-hint
      />

      <!-- Secret Access Key Field -->
      <v-text-field
        v-model="secretAccessKey"
        label="Secret Access Key"
        prepend-icon="mdi-key"
        :append-icon="showSecret ? 'mdi-eye' : 'mdi-eye-off'"
        :type="showSecret ? 'text' : 'password'"
        :rules="[rules.required]"
        @click:append="showSecret = !showSecret"
        clearable
        hint="Your AWS Secret Access Key from STS response"
        persistent-hint
      />

      <!-- Session Token Field -->
      <v-text-field
        v-model="sessionToken"
        label="Session Token"
        prepend-icon="mdi-clock-outline"
        :append-icon="showToken ? 'mdi-eye' : 'mdi-eye-off'"
        :type="showToken ? 'text' : 'password'"
        :rules="[rules.required]"
        @click:append="showToken = !showToken"
        clearable
        hint="Your AWS Session Token from STS response"
        persistent-hint
      />
    </v-card-text>

    <v-card-actions>
      <v-spacer />
      <v-btn
        color="secondary"
        variant="outlined"
        @click="handleTestConnection"
        :loading="testing"
        :disabled="!isValid"
      >
        <v-icon start>mdi-test-tube</v-icon>
        Test Connection
      </v-btn>
      <v-btn
        color="grey"
        variant="text"
        @click="$emit('close')"
      >
        Cancel
      </v-btn>
      <v-btn
        color="primary"
        @click="handleSave"
        :loading="saving"
        :disabled="!isValid"
      >
        <v-icon start>mdi-content-save</v-icon>
        Update & Retry
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script>
export default {
  name: 'RefreshAwsCredentials',
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      accessKeyId: '',
      secretAccessKey: '',
      sessionToken: '',
      showSecret: false,
      showToken: false,
      testing: false,
      saving: false,
      testResult: null,
      rules: {
        required: (value) => !!value || 'Required',
      },
    };
  },
  computed: {
    isValid() {
      return !!this.accessKeyId && !!this.secretAccessKey && !!this.sessionToken;
    },
    credentialMetadata() {
      return this.$store.getters['credentials/getMetadata']('AWS', this.serverId);
    },
  },
  methods: {
    async handleTestConnection() {
      if (!this.isValid) return;

      this.testing = true;
      this.testResult = null;

      try {
        const result = await this.$store.dispatch('credentials/testConnection', {
          serverId: parseInt(this.serverId, 10),
          serverType: 'AWS',
          username: this.accessKeyId,
          password: this.secretAccessKey,
          sessionToken: this.sessionToken,
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
      if (!this.isValid) return;

      this.saving = true;

      try {
        if (this.credentialMetadata) {
          // Update existing credentials
          await this.$store.dispatch('credentials/updateCredentials', {
            credentialId: this.credentialMetadata.id,
            username: this.accessKeyId,
            password: this.secretAccessKey,
            sessionToken: this.sessionToken,
            serverId: parseInt(this.serverId, 10),
            serverType: 'AWS',
          });
        } else {
          // Save new credentials
          await this.$store.dispatch('credentials/saveCredentials', {
            serverId: parseInt(this.serverId, 10),
            serverType: 'AWS',
            username: this.accessKeyId,
            password: this.secretAccessKey,
            sessionToken: this.sessionToken,
          });
        }

        // Clear fields
        this.accessKeyId = '';
        this.secretAccessKey = '';
        this.sessionToken = '';
        this.testResult = null;

        // Emit success and close
        this.$emit('saved');
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.saving = false;
      }
    },
  },
};
</script>

<style scoped>
.v-code {
  font-family: monospace;
  font-size: 0.875rem;
  border-radius: 4px;
}
</style>
