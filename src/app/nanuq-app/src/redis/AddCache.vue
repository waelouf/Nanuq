<template>
  <v-sheet class="mx-auto" width="300">
    <v-form fast-fail @submit.prevent>
      <v-text-field
        v-model="serverUrl"
        label="Server URL"
      />

      <v-text-field
        v-model="database"
        label="Database"
      />

      <v-text-field
        v-model="key"
        label="Key"
      />

      <v-text-field
        v-model="value"
        label="Value"
      />

      <v-text-field
        v-model="ttl"
        label="Time to live (in milliseconds)"
      />

      <v-btn class="mt-2" @click="addCache" type="submit" block>Save</v-btn>
      <br />
    </v-form>
  </v-sheet>
</template>
<script>
export default {
  name: 'AddCache',
  mounted() {
    this.serverUrl = this.selectedServerUrl;
    this.database = this.selectedDatabase;
  },
  props: {
    selectedServerUrl: {
      type: String,
      required: true,
    },
    selectedDatabase: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      serverUrl: '',
      database: '',
      key: '',
      value: '',
      ttl: -1,
    };
  },
  methods: {
    async addCache() {
      const cache = {
        serverUrl: this.serverUrl,
        database: this.database,
        key: this.key,
        value: this.value,
        ttlMilliseconds: this.ttl,
      };
      await this.$store.dispatch('redis/cacheString', cache);
      this.$emit('showModal', false);
    },
  },
};
</script>
