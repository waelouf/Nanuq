<template>
  <div>
    <v-select
      label="Database"
      :items="databases"
      v-model="selectedDatabase"
      @update:modelValue="onItemSelected" />

    <!-- Tabs for different data types -->
    <v-tabs v-model="activeTab" class="mt-4">
      <v-tab value="strings">
        <v-icon start>mdi-text</v-icon>
        Strings
      </v-tab>
      <v-tab value="lists">
        <v-icon start>mdi-format-list-numbered</v-icon>
        Lists
      </v-tab>
      <v-tab value="hashes">
        <v-icon start>mdi-code-braces</v-icon>
        Hashes
      </v-tab>
      <v-tab value="sets">
        <v-icon start>mdi-set-all</v-icon>
        Sets
      </v-tab>
      <v-tab value="sortedsets">
        <v-icon start>mdi-sort-numeric-variant</v-icon>
        Sorted Sets
      </v-tab>
      <v-tab value="streams">
        <v-icon start>mdi-waves</v-icon>
        Streams
      </v-tab>
    </v-tabs>

    <v-tabs-window v-model="activeTab" class="mt-4">
      <!-- Strings Tab -->
      <v-tabs-window-item value="strings">
        <div>
          <v-table v-if="databaseKeys && Object.keys(databaseKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key : value
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, value] in Object.entries(databaseKeys)"
                :key="key"
              >
                <td>{{ key }} : {{ value }}</td>
                <td>
                  <a @click="invalidateCache(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No string keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleAddCacheDialog(true)" type="submit" block>Add String</v-btn>
      </v-tabs-window-item>

      <!-- Lists Tab -->
      <v-tabs-window-item value="lists">
        <div>
          <v-table v-if="databaseListKeys && Object.keys(databaseListKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key
                </th>
                <th>
                  Length
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, length] in Object.entries(databaseListKeys)"
                :key="key"
              >
                <td>
                  <a @click="openManageList(key)" style="cursor: pointer; color: blue;">
                    {{ key }}
                  </a>
                </td>
                <td>{{ length }}</td>
                <td>
                  <a @click="deleteListKey(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No list keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleCreateListDialog(true)" type="submit" block>Create List</v-btn>
      </v-tabs-window-item>

      <!-- Hashes Tab -->
      <v-tabs-window-item value="hashes">
        <div>
          <v-table v-if="databaseHashKeys && Object.keys(databaseHashKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key
                </th>
                <th>
                  Fields
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, fieldCount] in Object.entries(databaseHashKeys)"
                :key="key"
              >
                <td>
                  <a @click="openManageHash(key)" style="cursor: pointer; color: blue;">
                    {{ key }}
                  </a>
                </td>
                <td>{{ fieldCount }}</td>
                <td>
                  <a @click="deleteHashKey(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No hash keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleCreateHashDialog(true)" type="submit" block>Create Hash</v-btn>
      </v-tabs-window-item>

      <!-- Sets Tab -->
      <v-tabs-window-item value="sets">
        <div>
          <v-table v-if="databaseSetKeys && Object.keys(databaseSetKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key
                </th>
                <th>
                  Members
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, memberCount] in Object.entries(databaseSetKeys)"
                :key="key"
              >
                <td>
                  <a @click="openManageSet(key)" style="cursor: pointer; color: blue;">
                    {{ key }}
                  </a>
                </td>
                <td>{{ memberCount }}</td>
                <td>
                  <a @click="deleteSetKey(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No set keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleCreateSetDialog(true)" type="submit" block>Create Set</v-btn>
      </v-tabs-window-item>

      <!-- Sorted Sets Tab -->
      <v-tabs-window-item value="sortedsets">
        <div>
          <v-table v-if="databaseSortedSetKeys && Object.keys(databaseSortedSetKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key
                </th>
                <th>
                  Members
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, memberCount] in Object.entries(databaseSortedSetKeys)"
                :key="key"
              >
                <td>
                  <a @click="openManageSortedSet(key)" style="cursor: pointer; color: blue;">
                    {{ key }}
                  </a>
                </td>
                <td>{{ memberCount }}</td>
                <td>
                  <a @click="deleteSortedSetKey(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No sorted set keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleCreateSortedSetDialog(true)" type="submit" block>Create Sorted Set</v-btn>
      </v-tabs-window-item>

      <!-- Streams Tab -->
      <v-tabs-window-item value="streams">
        <div>
          <v-table v-if="databaseStreamKeys && Object.keys(databaseStreamKeys).length > 0" width="300px">
            <thead>
              <tr>
                <th class="text-left">
                  Key
                </th>
                <th>
                  Entries
                </th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="[key, entryCount] in Object.entries(databaseStreamKeys)"
                :key="key"
              >
                <td>
                  <a @click="openManageStream(key)" style="cursor: pointer; color: blue;">
                    {{ key }}
                  </a>
                </td>
                <td>{{ entryCount }}</td>
                <td>
                  <a @click="deleteStreamKey(key)" class="delete-icon">
                    <i class="fa-regular fa-trash-can" />
                  </a>
                </td>
              </tr>
            </tbody>
          </v-table>
          <v-alert v-else type="info" variant="tonal" class="mt-2">
            No stream keys found in this database.
          </v-alert>
        </div>
        <v-btn class="mt-2" @click="handleCreateStreamDialog(true)" type="submit" block>Create Stream</v-btn>
      </v-tabs-window-item>
    </v-tabs-window>

    <!-- Add String Cache Dialog -->
    <v-dialog v-model="showAddCacheModal" width="600px">
      <v-card
        prepend-icon="mdi-update"
      >
        <AddCache
          :selectedServerUrl="serverUrl"
          :selectedDatabase="selectedDatabase"
          @showModal="show => handleAddCacheDialog(show)"
        />
      </v-card>
    </v-dialog>

    <!-- Create List Dialog -->
    <v-dialog v-model="showCreateListModal" width="400px">
      <v-card>
        <v-card-title>Create New List</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newListKey"
            label="List Key"
            prepend-icon="mdi-key"
          />
          <v-text-field
            v-model="newListValue"
            label="Initial Value (optional)"
            prepend-icon="mdi-text"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="handleCreateListDialog(false)">Cancel</v-btn>
          <v-btn color="primary" @click="createList" :disabled="!newListKey">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Manage List Dialog -->
    <v-dialog v-model="showManageListModal" width="800px">
      <v-card>
        <ManageList
          v-if="selectedListKey"
          :serverUrl="serverUrl"
          :database="selectedDatabase"
          :listKey="selectedListKey"
          @close="handleManageListDialog(false)"
        />
      </v-card>
    </v-dialog>

    <!-- Create Hash Dialog -->
    <v-dialog v-model="showCreateHashModal" width="400px">
      <v-card>
        <v-card-title>Create New Hash</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newHashKey"
            label="Hash Key"
            prepend-icon="mdi-key"
          />
          <v-text-field
            v-model="newHashField"
            label="Initial Field Name"
            prepend-icon="mdi-tag"
          />
          <v-text-field
            v-model="newHashValue"
            label="Initial Field Value"
            prepend-icon="mdi-text"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="handleCreateHashDialog(false)">Cancel</v-btn>
          <v-btn color="primary" @click="createHash" :disabled="!newHashKey || !newHashField || !newHashValue">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Manage Hash Dialog -->
    <v-dialog v-model="showManageHashModal" width="800px">
      <v-card>
        <ManageHash
          v-if="selectedHashKey"
          :serverUrl="serverUrl"
          :database="selectedDatabase"
          :hashKey="selectedHashKey"
          @close="handleManageHashDialog(false)"
        />
      </v-card>
    </v-dialog>

    <!-- Create Set Dialog -->
    <v-dialog v-model="showCreateSetModal" width="400px">
      <v-card>
        <v-card-title>Create New Set</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newSetKey"
            label="Set Key"
            prepend-icon="mdi-key"
          />
          <v-text-field
            v-model="newSetMember"
            label="Initial Member"
            prepend-icon="mdi-text"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="handleCreateSetDialog(false)">Cancel</v-btn>
          <v-btn color="primary" @click="createSet" :disabled="!newSetKey || !newSetMember">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Manage Set Dialog -->
    <v-dialog v-model="showManageSetModal" width="800px">
      <v-card>
        <ManageSet
          v-if="selectedSetKey"
          :serverUrl="serverUrl"
          :database="selectedDatabase"
          :setKey="selectedSetKey"
          @close="handleManageSetDialog(false)"
        />
      </v-card>
    </v-dialog>

    <!-- Create Sorted Set Dialog -->
    <v-dialog v-model="showCreateSortedSetModal" width="400px">
      <v-card>
        <v-card-title>Create New Sorted Set</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newSortedSetKey"
            label="Sorted Set Key"
            prepend-icon="mdi-key"
          />
          <v-text-field
            v-model="newSortedSetMember"
            label="Initial Member"
            prepend-icon="mdi-text"
          />
          <v-text-field
            v-model.number="newSortedSetScore"
            label="Initial Score"
            type="number"
            prepend-icon="mdi-numeric"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="handleCreateSortedSetDialog(false)">Cancel</v-btn>
          <v-btn color="primary" @click="createSortedSet" :disabled="!newSortedSetKey || !newSortedSetMember || newSortedSetScore === null || newSortedSetScore === ''">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Manage Sorted Set Dialog -->
    <v-dialog v-model="showManageSortedSetModal" width="800px">
      <v-card>
        <ManageSortedSet
          v-if="selectedSortedSetKey"
          :serverUrl="serverUrl"
          :database="selectedDatabase"
          :sortedSetKey="selectedSortedSetKey"
          @close="handleManageSortedSetDialog(false)"
        />
      </v-card>
    </v-dialog>

    <!-- Create Stream Dialog -->
    <v-dialog v-model="showCreateStreamModal" width="600px">
      <v-card>
        <v-card-title>Create New Stream</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newStreamKey"
            label="Stream Key"
            prepend-icon="mdi-key"
            class="mb-3"
          />
          <div class="text-subtitle-2 mb-2">Initial Entry Fields:</div>
          <div v-for="(field, index) in newStreamFields" :key="index" class="d-flex gap-2 mb-2 align-center">
            <v-text-field
              v-model="field.key"
              label="Field Name"
              prepend-icon="mdi-tag"
              density="compact"
            />
            <v-text-field
              v-model="field.value"
              label="Field Value"
              prepend-icon="mdi-text"
              density="compact"
            />
            <v-btn
              v-if="newStreamFields.length > 1"
              color="error"
              variant="text"
              icon
              size="small"
              @click="removeStreamField(index)"
            >
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </div>
          <v-btn
            color="secondary"
            variant="outlined"
            size="small"
            @click="addStreamField"
            class="mt-2"
          >
            <v-icon start>mdi-plus</v-icon>
            Add Field
          </v-btn>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="secondary" @click="handleCreateStreamDialog(false)">Cancel</v-btn>
          <v-btn color="primary" @click="createStream" :disabled="!newStreamKey || !newStreamFields.some(f => f.key && f.value)">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Manage Stream Dialog -->
    <v-dialog v-model="showManageStreamModal" width="900px">
      <v-card>
        <ManageStream
          v-if="selectedStreamKey"
          :serverUrl="serverUrl"
          :database="selectedDatabase"
          :streamKey="selectedStreamKey"
          @close="handleManageStreamDialog(false)"
        />
      </v-card>
    </v-dialog>
  </div>
</template>
<script>
import AddCache from './AddCache.vue';
import ManageList from './ManageList.vue';
import ManageHash from './ManageHash.vue';
import ManageSet from './ManageSet.vue';
import ManageSortedSet from './ManageSortedSet.vue';
import ManageStream from './ManageStream.vue';

export default {
  name: 'ManageDatabase',
  components: { AddCache, ManageList, ManageHash, ManageSet, ManageSortedSet, ManageStream },
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
  },
  computed: {
    serverDetails() {
      return this.$store.state.redis.redisServers[this.serverUrl];
    },
    databaseKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisDatabaseKeys[key];
    },
    databaseListKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisListKeys[key];
    },
    databaseHashKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisHashKeys[key];
    },
    databaseSetKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisSetKeys[key];
    },
    databaseSortedSetKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisSortedSetKeys[key];
    },
    databaseStreamKeys() {
      const key = `${this.serverUrl}_${this.selectedDatabase}`;
      return this.$store.state.redis.redisStreamKeys[key];
    },
  },
  data() {
    return {
      databases: [],
      selectedDatabase: '',
      showAddCacheModal: false,
      activeTab: 'strings',
      showCreateListModal: false,
      showManageListModal: false,
      newListKey: '',
      newListValue: '',
      selectedListKey: '',
      showCreateHashModal: false,
      showManageHashModal: false,
      newHashKey: '',
      newHashField: '',
      newHashValue: '',
      selectedHashKey: '',
      showCreateSetModal: false,
      showManageSetModal: false,
      newSetKey: '',
      newSetMember: '',
      selectedSetKey: '',
      showCreateSortedSetModal: false,
      showManageSortedSetModal: false,
      newSortedSetKey: '',
      newSortedSetMember: '',
      newSortedSetScore: null,
      selectedSortedSetKey: '',
      showCreateStreamModal: false,
      showManageStreamModal: false,
      newStreamKey: '',
      newStreamFields: [{ key: '', value: '' }],
      selectedStreamKey: '',
    };
  },
  async mounted() {
    await this.loadServerDetails();
    // Wait for next tick to ensure computed property is updated
    await this.$nextTick();
    // Check if serverDetails exists before accessing databaseCount
    if (this.serverDetails && this.serverDetails.databaseCount) {
      // eslint-disable-next-line no-plusplus
      for (let i = 0; i < Number(this.serverDetails.databaseCount); i++) {
        this.databases.push(i);
      }
    }
  },
  methods: {
    async loadServerDetails() {
      await this.$store.dispatch('redis/getServerDetails', this.serverUrl);
    },
    onItemSelected() {
      this.loadDatabaseKeys();
      this.loadDatabaseListKeys();
      this.loadDatabaseHashKeys();
      this.loadDatabaseSetKeys();
      this.loadDatabaseSortedSetKeys();
      this.loadDatabaseStreamKeys();
    },
    async loadDatabaseKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllStringKeys', requestObject);
    },
    async loadDatabaseListKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllListKeys', requestObject);
    },
    async loadDatabaseHashKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllHashKeys', requestObject);
    },
    async invalidateCache(key) {
      const invalidateCacheObject = {
        serverUrl: this.serverUrl,
        database: this.selectedDatabase,
        key,
      };
      await this.$store.dispatch('redis/invalidateCachedString', invalidateCacheObject);
      await this.loadDatabaseKeys();
    },
    handleAddCacheDialog(show) {
      this.showAddCacheModal = show;
      if (!show) {
        this.loadDatabaseKeys();
      }
    },
    handleCreateListDialog(show) {
      this.showCreateListModal = show;
      if (!show) {
        this.newListKey = '';
        this.newListValue = '';
      }
    },
    async createList() {
      try {
        await this.$store.dispatch('redis/pushListElement', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key: this.newListKey,
          value: this.newListValue || 'initial',
          pushLeft: true,
        });
        this.handleCreateListDialog(false);
        await this.loadDatabaseListKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    openManageList(key) {
      this.selectedListKey = key;
      this.showManageListModal = true;
    },
    handleManageListDialog(show) {
      this.showManageListModal = show;
      if (!show) {
        this.selectedListKey = '';
        this.loadDatabaseListKeys();
      }
    },
    async deleteListKey(key) {
      try {
        await this.$store.dispatch('redis/deleteList', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key,
        });
        await this.loadDatabaseListKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    handleCreateHashDialog(show) {
      this.showCreateHashModal = show;
      if (!show) {
        this.newHashKey = '';
        this.newHashField = '';
        this.newHashValue = '';
      }
    },
    async createHash() {
      try {
        await this.$store.dispatch('redis/setHashField', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key: this.newHashKey,
          field: this.newHashField,
          value: this.newHashValue,
        });
        this.handleCreateHashDialog(false);
        await this.loadDatabaseHashKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    openManageHash(key) {
      this.selectedHashKey = key;
      this.showManageHashModal = true;
    },
    handleManageHashDialog(show) {
      this.showManageHashModal = show;
      if (!show) {
        this.selectedHashKey = '';
        this.loadDatabaseHashKeys();
      }
    },
    async deleteHashKey(key) {
      try {
        await this.$store.dispatch('redis/deleteHash', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key,
        });
        await this.loadDatabaseHashKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    async loadDatabaseSetKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllSetKeys', requestObject);
    },
    handleCreateSetDialog(show) {
      this.showCreateSetModal = show;
      if (!show) {
        this.newSetKey = '';
        this.newSetMember = '';
      }
    },
    async createSet() {
      try {
        await this.$store.dispatch('redis/addSetMember', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key: this.newSetKey,
          member: this.newSetMember,
        });
        this.handleCreateSetDialog(false);
        await this.loadDatabaseSetKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    openManageSet(key) {
      this.selectedSetKey = key;
      this.showManageSetModal = true;
    },
    handleManageSetDialog(show) {
      this.showManageSetModal = show;
      if (!show) {
        this.selectedSetKey = '';
        this.loadDatabaseSetKeys();
      }
    },
    async deleteSetKey(key) {
      try {
        await this.$store.dispatch('redis/deleteSet', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key,
        });
        await this.loadDatabaseSetKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    async loadDatabaseSortedSetKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllSortedSetKeys', requestObject);
    },
    handleCreateSortedSetDialog(show) {
      this.showCreateSortedSetModal = show;
      if (!show) {
        this.newSortedSetKey = '';
        this.newSortedSetMember = '';
        this.newSortedSetScore = null;
      }
    },
    async createSortedSet() {
      try {
        await this.$store.dispatch('redis/addSortedSetMember', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key: this.newSortedSetKey,
          member: this.newSortedSetMember,
          score: parseFloat(this.newSortedSetScore),
        });
        this.handleCreateSortedSetDialog(false);
        await this.loadDatabaseSortedSetKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    openManageSortedSet(key) {
      this.selectedSortedSetKey = key;
      this.showManageSortedSetModal = true;
    },
    handleManageSortedSetDialog(show) {
      this.showManageSortedSetModal = show;
      if (!show) {
        this.selectedSortedSetKey = '';
        this.loadDatabaseSortedSetKeys();
      }
    },
    async deleteSortedSetKey(key) {
      try {
        await this.$store.dispatch('redis/deleteSortedSet', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key,
        });
        await this.loadDatabaseSortedSetKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    async loadDatabaseStreamKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllStreamKeys', requestObject);
    },
    handleCreateStreamDialog(show) {
      this.showCreateStreamModal = show;
      if (!show) {
        this.newStreamKey = '';
        this.newStreamFields = [{ key: '', value: '' }];
      }
    },
    addStreamField() {
      this.newStreamFields.push({ key: '', value: '' });
    },
    removeStreamField(index) {
      this.newStreamFields.splice(index, 1);
    },
    async createStream() {
      try {
        const fields = {};
        this.newStreamFields.forEach((field) => {
          if (field.key && field.value) {
            fields[field.key] = field.value;
          }
        });

        if (Object.keys(fields).length === 0) {
          return;
        }

        await this.$store.dispatch('redis/addStreamEntry', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key: this.newStreamKey,
          fields,
        });
        this.handleCreateStreamDialog(false);
        await this.loadDatabaseStreamKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
    openManageStream(key) {
      this.selectedStreamKey = key;
      this.showManageStreamModal = true;
    },
    handleManageStreamDialog(show) {
      this.showManageStreamModal = show;
      if (!show) {
        this.selectedStreamKey = '';
        this.loadDatabaseStreamKeys();
      }
    },
    async deleteStreamKey(key) {
      try {
        await this.$store.dispatch('redis/deleteStream', {
          serverUrl: this.serverUrl,
          database: this.selectedDatabase,
          key,
        });
        await this.loadDatabaseStreamKeys();
      } catch (error) {
        // Error is already handled by the store
      }
    },
  },
};
</script>
