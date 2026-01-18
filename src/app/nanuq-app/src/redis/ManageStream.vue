<template>
  <v-sheet class="mx-auto pa-4" width="100%" style="position: relative;">
    <!-- Loading Overlay -->
    <v-overlay :model-value="loading" contained class="align-center justify-center">
      <v-progress-circular indeterminate size="64" color="primary" />
    </v-overlay>

    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2">mdi-waves</v-icon>
      Manage Stream: {{ streamKey }}
      <v-spacer />
      <v-chip color="primary" size="small" class="ml-2">
        Entries: {{ entries.length }}
      </v-chip>
    </v-card-title>

    <!-- Add Entry Form -->
    <v-card class="mb-4 mt-4" variant="outlined">
      <v-card-title class="text-subtitle-1">Add Entry</v-card-title>
      <v-card-text>
        <div v-for="(field, index) in newEntryFields" :key="index" class="d-flex gap-2 mb-2 align-center">
          <v-text-field
            v-model="field.key"
            label="Field Name"
            prepend-icon="mdi-key"
            clearable
            density="compact"
          />
          <v-text-field
            v-model="field.value"
            label="Field Value"
            prepend-icon="mdi-text"
            clearable
            density="compact"
          />
          <v-btn
            v-if="newEntryFields.length > 1"
            color="error"
            variant="text"
            icon
            size="small"
            @click="removeField(index)"
          >
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </div>
        <div class="d-flex gap-2 mt-2">
          <v-btn
            color="secondary"
            variant="outlined"
            size="small"
            @click="addField"
          >
            <v-icon start>mdi-plus</v-icon>
            Add Field
          </v-btn>
          <v-btn
            color="primary"
            @click="addEntry"
            :disabled="!canAddEntry"
          >
            <v-icon start>mdi-plus</v-icon>
            Add Entry
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Error Alert -->
    <v-alert v-if="error" type="error" variant="tonal" class="mb-4" closable @click:close="error = null">
      {{ error }}
      <template #append>
        <v-btn color="error" variant="text" @click="loadEntries">Retry</v-btn>
      </template>
    </v-alert>

    <!-- Stream Entries Table -->
    <v-card variant="outlined">
      <v-card-title class="text-subtitle-1">Entries (newest first)</v-card-title>
      <v-card-text>
        <v-table v-if="entries.length > 0">
          <thead>
            <tr>
              <th>Entry ID</th>
              <th>Fields</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="entry in entries" :key="entry.id">
              <td><small>{{ entry.id }}</small></td>
              <td>
                <div v-for="(value, key) in entry.fields" :key="key" class="mb-1">
                  <strong>{{ key }}:</strong> {{ value }}
                </div>
              </td>
            </tr>
          </tbody>
        </v-table>
        <v-alert v-else type="info" variant="tonal">
          This stream has no entries.
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Actions -->
    <div class="d-flex justify-end gap-2 mt-4">
      <v-btn color="error" @click="confirmDeleteStream = true">
        <v-icon start>mdi-delete</v-icon>
        Delete Stream
      </v-btn>
      <v-btn color="secondary" variant="outlined" @click="closeDialog">
        Close
      </v-btn>
    </div>

    <!-- Delete Stream Confirmation Dialog -->
    <v-dialog v-model="confirmDeleteStream" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this entire stream? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDeleteStream = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteStream">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
export default {
  name: 'ManageStream',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    database: {
      type: Number,
      required: true,
    },
    streamKey: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      entries: [],
      newEntryFields: [
        { key: '', value: '' },
      ],
      confirmDeleteStream: false,
      loading: false,
      error: null,
    };
  },
  computed: {
    canAddEntry() {
      return this.newEntryFields.some((field) => field.key && field.value);
    },
  },
  async mounted() {
    await this.loadEntries();
  },
  methods: {
    async loadEntries() {
      try {
        this.loading = true;
        this.error = null;
        this.entries = await this.$store.dispatch('redis/getStreamEntries', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.streamKey,
          count: 100,
        });
      } catch (error) {
        this.error = 'Failed to load stream entries. Please try again.';
        console.error('Error loading stream entries:', error);
      } finally {
        this.loading = false;
      }
    },
    addField() {
      this.newEntryFields.push({ key: '', value: '' });
    },
    removeField(index) {
      this.newEntryFields.splice(index, 1);
    },
    async addEntry() {
      try {
        this.loading = true;
        const fields = {};
        this.newEntryFields.forEach((field) => {
          if (field.key && field.value) {
            fields[field.key] = field.value;
          }
        });

        if (Object.keys(fields).length === 0) {
          return;
        }

        await this.$store.dispatch('redis/addStreamEntry', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.streamKey,
          fields,
        });

        this.newEntryFields = [{ key: '', value: '' }];
        await this.loadEntries();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async deleteStream() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/deleteStream', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.streamKey,
        });
        this.confirmDeleteStream = false;
        this.closeDialog();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
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
