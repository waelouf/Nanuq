<template>
  <v-sheet class="mx-auto pa-4" width="100%" style="position: relative;">
    <!-- Loading Overlay -->
    <v-overlay :model-value="loading" contained class="align-center justify-center">
      <v-progress-circular indeterminate size="64" color="primary" />
    </v-overlay>

    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2">mdi-set-all</v-icon>
      Manage Set: {{ setKey }}
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
          @keyup.enter="addMember"
        />
        <div class="d-flex gap-2 mt-2">
          <v-btn
            color="primary"
            @click="addMember"
            :disabled="!newMember"
          >
            <v-icon start>mdi-plus</v-icon>
            Add Member
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

    <!-- Set Members Table -->
    <v-card variant="outlined">
      <v-card-title class="text-subtitle-1">Members</v-card-title>
      <v-card-text>
        <v-table v-if="members.length > 0">
          <thead>
            <tr>
              <th>Member</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="member in members" :key="member">
              <td>{{ member }}</td>
              <td>
                <v-btn
                  size="small"
                  color="error"
                  variant="text"
                  @click="confirmRemoveMember(member)"
                  icon
                >
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </td>
            </tr>
          </tbody>
        </v-table>
        <v-alert v-else type="info" variant="tonal">
          This set has no members.
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Actions -->
    <div class="d-flex justify-end gap-2 mt-4">
      <v-btn color="error" @click="confirmDeleteSet = true">
        <v-icon start>mdi-delete</v-icon>
        Delete Set
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

    <!-- Delete Set Confirmation Dialog -->
    <v-dialog v-model="confirmDeleteSet" max-width="400">
      <v-card>
        <v-card-title>Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this entire set? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="confirmDeleteSet = false">Cancel</v-btn>
          <v-btn color="error" @click="deleteSet">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-sheet>
</template>

<script>
export default {
  name: 'ManageSet',
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
    database: {
      type: Number,
      required: true,
    },
    setKey: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      members: [],
      newMember: '',
      confirmRemoveMemberDialog: false,
      confirmDeleteSet: false,
      memberToRemove: '',
      loading: false,
    };
  },
  async mounted() {
    await this.loadMembers();
  },
  methods: {
    async loadMembers() {
      try {
        this.loading = true;
        this.members = await this.$store.dispatch('redis/getSetMembers', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.setKey,
        });
      } catch (error) {
        // Error is already handled by the store
      } finally {
        this.loading = false;
      }
    },
    async addMember() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/addSetMember', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.setKey,
          member: this.newMember,
        });
        this.newMember = '';
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
        await this.$store.dispatch('redis/removeSetMember', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.setKey,
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
    async deleteSet() {
      try {
        this.loading = true;
        await this.$store.dispatch('redis/deleteSet', {
          serverUrl: this.serverUrl,
          database: this.database,
          key: this.setKey,
        });
        this.confirmDeleteSet = false;
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
