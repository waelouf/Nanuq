<template>
  <v-sheet class="mx-auto pa-4" width="100%">
    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2">mdi-code-braces</v-icon>
      Manage Hash: {{ hashKey }}
      <v-spacer />
      <v-chip color="primary" size="small" class="ml-2">
        Fields: {{ Object.keys(fields).length }}
      </v-chip>
    </v-card-title>

    <!-- Add/Edit Field Form -->
    <v-card class="mb-4 mt-4" variant="outlined">
      <v-card-title class="text-subtitle-1">{{ editingField ? 'Edit Field' : 'Add Field' }}</v-card-title>
      <v-card-text>
        <v-text-field
          v-model="newField"
          label="Field Name"
          prepend-icon="mdi-key"
          clearable
          :disabled="!!editingField"
        />
        <v-text-field
          v-model="newValue"
          label="Field Value"
          prepend-icon="mdi-text"
          clearable
          @keyup.enter="saveField"
        />
        <div class="d-flex gap-2 mt-2">
          <v-btn
            v-if="editingField"
            color="secondary"
            variant="outlined"
            @click="cancelEdit"
          >
            Cancel
          </v-btn>
          <v-btn
            color="primary"
            @click="saveField"
            :disabled="!newField || !newValue"
          >
            <v-icon start>mdi-content-save</v-icon>
            {{ editingField ? 'Update' : 'Add' }} Field
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Hash Fields Table -->
    <v-card variant="outlined">
      <v-card-title class="text-subtitle-1">Fields</v-card-title>
      <v-card-text>
        <v-table v-if="Object.keys(fields).length > 0">
          <thead>
            <tr>
              <th>Field</th>
              <th>Value</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(value, field) in fields" :key="field">
              <td><strong>{{ field }}</strong></td>
              <td>{{ value }}</td>
              <td>
                <v-btn
                  size="small"
                  color="primary"
                  variant="text"
                  @click="editField(field, value)"
                  icon
                >
                  <v-icon>mdi-pencil</v-icon>
                </v-btn>
                <v-btn
                  size="small"
                  color="error"
                  variant="text"
                  @click="confirmDeleteField(field)"
                  icon
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </td>
            </tr>
          </tbody>
        </v-table>
        <v-alert v-else type="info" variant="tonal">
          This hash has no fields.
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Actions -->
    <div class="d-flex justify-end gap-2 mt-4">
      <v-btn color="error" @click="confirmDeleteHash = true">
        <v-icon start>mdi-delete</v-icon>
        Delete Hash
      </v-btn>
      <v-btn color="secondary" variant="outlined" @click="closeDialog">
        Close
      </v-btn>
    </div>

    <!-- Delete Field Confirmation Dialog -->
    <v-dialog v-model="confirmDeleteFieldDialog" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete Field</v-card-title>
        <v-card-text>
          Are you sure you want to delete the field "{{ fieldToDelete }}"?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDeleteFieldDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteField">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Hash Confirmation Dialog -->
    <v-dialog v-model="confirmDeleteHash" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this entire hash? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDeleteHash = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteHash">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
export default {
  name: 'ManageHash',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    database: {
      type: Number,
      required: true,
    },
    hashKey: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      fields: {},
      newField: '',
      newValue: '',
      editingField: null,
      confirmDeleteFieldDialog: false,
      confirmDeleteHash: false,
      fieldToDelete: '',
    };
  },
  async mounted() {
    await this.loadFields();
  },
  methods: {
    async loadFields() {
      try {
        this.fields = await this.$store.dispatch('redis/getHashAllFields', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.hashKey,
        });
      } catch (error) {
        // Error is already handled by the store
      }
    },
    async saveField() {
      try {
        await this.$store.dispatch('redis/setHashField', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.hashKey,
          field: this.newField,
          value: this.newValue,
        });
        this.newField = '';
        this.newValue = '';
        this.editingField = null;
        await this.loadFields();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    editField(field, value) {
      this.editingField = field;
      this.newField = field;
      this.newValue = value;
    },
    cancelEdit() {
      this.editingField = null;
      this.newField = '';
      this.newValue = '';
    },
    confirmDeleteField(field) {
      this.fieldToDelete = field;
      this.confirmDeleteFieldDialog = true;
    },
    async deleteField() {
      try {
        await this.$store.dispatch('redis/deleteHashField', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.hashKey,
          field: this.fieldToDelete,
        });
        this.confirmDeleteFieldDialog = false;
        this.fieldToDelete = '';
        await this.loadFields();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    async deleteHash() {
      try {
        await this.$store.dispatch('redis/deleteHash', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.hashKey,
        });
        this.confirmDeleteHash = false;
        this.closeDialog();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    closeDialog() {
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
