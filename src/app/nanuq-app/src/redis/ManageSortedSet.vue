<template>
  <v-sheet class="mx-auto pa-4" width="100%" style="position: relative;">
    <!-- Loading Overlay -->
    <v-overlay :model-value="loading" contained class="align-center justify-center">
      <v-progress-circular indeterminate size="64" color="primary" />
    </v-overlay>

    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2">mdi-sort-numeric-variant</v-icon>
      Manage Sorted Set: {{ sortedSetKey }}
      <v-spacer />
      <v-chip color="primary" size="small" class="ml-2">
        Members: {{ members.length }}
      </v-chip>
    </v-card-title>

    <!-- Add Member Form -->
    <v-card class="mb-4 mt-4" variant="outlined">
      <v-card-title class="text-subtitle-1">Add Member</v-card-title>
      <v-card-text>
        <v-text-field
          v-model="newMember"
          label="Member Value"
          prepend-icon="mdi-text"
          clearable
        />
        <v-text-field
          v-model.number="newScore"
          label="Score"
          type="number"
          prepend-icon="mdi-numeric"
          clearable
          @keyup.enter="addMember"
        />
        <div class="d-flex gap-2 mt-2">
          <v-btn
            color="primary"
            @click="addMember"
            :disabled="!newMember || newScore === null || newScore === ''"
          >
            <v-icon start>mdi-plus</v-icon>
            Add Member
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Error Alert -->
    <v-alert v-if="error" type="error" variant="tonal" class="mb-4" closable @click:close="error = null">
      {{ error }}
      <template #append>
        <v-btn color="error" variant="text" @click="loadMembers">Retry</v-btn>
      </template>
    </v-alert>

    <!-- Sorted Set Members Table -->
    <v-card variant="outlined">
      <v-card-title class="text-subtitle-1">Members (sorted by score)</v-card-title>
      <v-card-text>
        <v-table v-if="members.length > 0">
          <thead>
            <tr>
              <th>Member</th>
              <th>Score</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, index) in members" :key="index">
              <td>{{ item.member }}</td>
              <td>{{ item.score }}</td>
              <td>
                <v-btn
                  size="small"
                  color="error"
                  variant="text"
                  @click="confirmRemoveMember(item.member)"
                  icon
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </td>
            </tr>
          </tbody>
        </v-table>
        <v-alert v-else type="info" variant="tonal">
          This sorted set has no members.
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Actions -->
    <div class="d-flex justify-end gap-2 mt-4">
      <v-btn color="error" @click="confirmDeleteSortedSet = true">
        <v-icon start>mdi-delete</v-icon>
        Delete Sorted Set
      </v-btn>
      <v-btn color="secondary" variant="outlined" @click="closeDialog">
        Close
      </v-btn>
    </div>

    <!-- Remove Member Confirmation Dialog -->
    <v-dialog v-model="confirmRemoveMemberDialog" max-width="400">
      <v-card>
        <v-card-title>Confirm Remove Member</v-card-title>
        <v-card-text>
          Are you sure you want to remove the member "{{ memberToRemove }}"?
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmRemoveMemberDialog = false">Cancel</v-btn>
          <v-btn color="error" @click="removeMember">Remove</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Sorted Set Confirmation Dialog -->
    <v-dialog v-model="confirmDeleteSortedSet" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this entire sorted set? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDeleteSortedSet = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteSortedSet">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
export default {
  name: 'ManageSortedSet',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    database: {
      type: Number,
      required: true,
    },
    sortedSetKey: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      members: [],
      newMember: '',
      newScore: null,
      confirmRemoveMemberDialog: false,
      confirmDeleteSortedSet: false,
      memberToRemove: '',
      loading: false,
      error: null,
    };
  },
  async mounted() {
    await this.loadMembers();
  },
  methods: {
    async loadMembers() {
      try {
        this.loading = true;
        this.error = null;
        const rawMembers = await this.$store.dispatch('redis/getSortedSetMembers', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.sortedSetKey,
        });
        // Convert the API response format to the component format
        this.members = rawMembers.map((item) => ({
          member: item.item1,
          score: item.item2,
        }));
      } catch (error) {
        this.error = 'Failed to load sorted set members. Please try again.';
        console.error('Error loading sorted set members:', error);
      } finally {
        this.loading = false;
      }
    },
    async addMember() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/addSortedSetMember', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.sortedSetKey,
          member: this.newMember,
          score: parseFloat(this.newScore),
        });
        this.newMember = '';
        this.newScore = null;
        await this.loadMembers();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    confirmRemoveMember(member) {
      this.memberToRemove = member;
      this.confirmRemoveMemberDialog = true;
    },
    async removeMember() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/removeSortedSetMember', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.sortedSetKey,
          member: this.memberToRemove,
        });
        this.confirmRemoveMemberDialog = false;
        this.memberToRemove = '';
        await this.loadMembers();
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async deleteSortedSet() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/deleteSortedSet', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.sortedSetKey,
        });
        this.confirmDeleteSortedSet = false;
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
