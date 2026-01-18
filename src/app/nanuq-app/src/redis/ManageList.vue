<template>
  <v-sheet class="mx-auto pa-4" width="100%" style="position: relative;">
    <!-- Loading Overlay -->
    <v-overlay :model-value="loading" contained class="align-center justify-center">
      <v-progress-circular indeterminate size="64" color="primary" />
    </v-overlay>

    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2">mdi-format-list-numbered</v-icon>
      Manage List: {{ listKey }}
      <v-spacer />
      <v-chip color="primary" size="small" class="ml-2">
        Length: {{ elements.length }}
      </v-chip>
    </v-card-title>

    <!-- Add Element Form -->
    <v-card class="mb-4 mt-4" variant="outlined">
      <v-card-title class="text-subtitle-1">Add Element</v-card-title>
      <v-card-text>
        <v-text-field
          v-model="newElement"
          label="Element Value"
          prepend-icon="mdi-text"
          clearable
          @keyup.enter="pushElement(true)"
        />
        <div class="d-flex gap-2 mt-2">
          <v-btn
            color="primary"
            @click="pushElement(true)"
            :disabled="!newElement"
          >
            <v-icon start>mdi-arrow-left-bold</v-icon>
            Push Left (Head)
          </v-btn>
          <v-btn
            color="primary"
            @click="pushElement(false)"
            :disabled="!newElement"
          >
            <v-icon start>mdi-arrow-right-bold</v-icon>
            Push Right (Tail)
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Pagination Info -->
    <v-alert v-if="elements.length >= 100" type="info" variant="tonal" class="mb-4">
      Showing first 100 elements. Total length can be viewed in the element count badge above.
    </v-alert>

    <!-- List Elements Table -->
    <v-card variant="outlined">
      <v-card-title class="text-subtitle-1">Elements</v-card-title>
      <v-card-text>
        <v-table v-if="elements.length > 0">
          <thead>
            <tr>
              <th>Index</th>
              <th>Value</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(element, index) in elements" :key="index">
              <td>{{ index }}</td>
              <td>{{ element }}</td>
            </tr>
          </tbody>
        </v-table>
        <v-alert v-else type="info" variant="tonal">
          This list is empty.
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Actions -->
    <div class="d-flex justify-end gap-2 mt-4">
      <v-btn
        color="secondary"
        variant="outlined"
        @click="popElement(true)"
        :disabled="elements.length === 0"
      >
        <v-icon start>mdi-arrow-left-bold</v-icon>
        Pop Left
      </v-btn>
      <v-btn
        color="secondary"
        variant="outlined"
        @click="popElement(false)"
        :disabled="elements.length === 0"
      >
        <v-icon start>mdi-arrow-right-bold</v-icon>
        Pop Right
      </v-btn>
      <v-btn color="error" @click="confirmDelete = true">
        <v-icon start>mdi-delete</v-icon>
        Delete List
      </v-btn>
      <v-btn color="secondary" variant="outlined" @click="closeDialog">
        Close
      </v-btn>
    </div>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="confirmDelete" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this entire list? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDelete = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteList">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
export default {
  name: 'ManageList',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    database: {
      type: Number,
      required: true,
    },
    listKey: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      elements: [],
      newElement: '',
      confirmDelete: false,
      loading: false,
    };
  },
  async mounted() {
    await this.loadElements();
  },
  methods: {
    async loadElements() {
      try {
        this.loading = true;
        this.elements = await this.$store.dispatch('redis/getListElements', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.listKey,
        });
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async pushElement(pushLeft) {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/pushListElement', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.listKey,
          value: this.newElement,
          pushLeft,
        });
        this.newElement = '';
        await this.loadElements();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async popElement(popLeft) {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/popListElement', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.listKey,
          popLeft,
        });
        await this.loadElements();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async deleteList() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/deleteList', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.listKey,
        });
        this.confirmDelete = false;
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
