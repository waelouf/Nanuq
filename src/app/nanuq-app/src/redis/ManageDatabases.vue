<template>
    <div>
        <v-select label="Database"
        :items="databases"
        v-model="selectedDatabase"
        @update:modelValue="onItemSelected">
        </v-select>
        <div>
            <v-table v-if="databaseKeys" width="300px">
                <thead>
                  <tr>
                    <th class="text-left">
                      Key : value
                    </th>
                    <th></th>
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
                            <i class="fa-regular fa-trash-can"></i>
                          </a>
                    </td>
                  </tr>
                </tbody>
              </v-table>
        </div>
        <v-btn class="mt-2" @click="handleAddCacheDialog(true)" type="submit" block>Add cache</v-btn>
        <v-dialog v-model="showAddCacheModal" width="600px" >
            <v-card
            prepend-icon="mdi-update"
          >
            <AddCache :selectedServerUrl="serverUrl" :selectedDatabase="selectedDatabase"
            @showModal="show => handleAddCacheDialog(show)"
            >

            </AddCache>
          </v-card>
          </v-dialog>
    </div>
</template>
<script>
import AddCache from './AddCache.vue';

export default {
  name: 'ManageDatabase',
  components: { AddCache },
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
  },
  data() {
    return {
      databases: [],
      selectedDatabase: '',
      showAddCacheModal: false,
    };
  },
  async mounted() {
    await this.loadServerDetails();
    // eslint-disable-next-line no-plusplus
    for (let i = 0; i < Number(this.serverDetails.databaseCount); i++) {
      this.databases.push(i);
    }
  },
  methods: {
    async loadServerDetails() {
      await this.$store.dispatch('redis/getServerDetails', this.serverUrl);
    },
    onItemSelected() {
      this.loadDatabaseKeys();
    },
    async loadDatabaseKeys() {
      const requestObject = { serverUrl: this.serverUrl, database: this.selectedDatabase };
      await this.$store.dispatch('redis/getAllStringKeys', requestObject);
    },
    async invalidateCache(key) {
      const invalidateCacheObject = { serverUrl: this.serverUrl, database: this.selectedDatabase, key };
      await this.$store.dispatch('redis/invalidateCachedString', invalidateCacheObject);
      await this.loadDatabaseKeys();
    },
    handleAddCacheDialog(show) {
      this.showAddCacheModal = show;
      if (!show) {
        this.loadDatabaseKeys();
      }
    },
  },
};
</script>
