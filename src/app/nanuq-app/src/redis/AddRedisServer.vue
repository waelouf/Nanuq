<template>
  <v-sheet class="mx-auto" width="75%">
    <v-form fast-fail @submit.prevent>
      <v-text-field
        v-model="serverUrl"
        label="Server URL"
      />

      <v-text-field
        v-model="alias"
        label="Alias"
      />

      <v-btn class="mt-2" @click="saveServer" type="submit" block>Save</v-btn>
      <br />
    </v-form>
  </v-sheet>
</template>
<script>

export default {
  name: 'AddServer',
  data() {
    return {
      serverUrl: '',
      alias: '',
    };
  },
  methods: {
    async saveServer() {
      const serverDetails = { serverUrl: this.serverUrl, alias: this.alias };
      await this.$store.dispatch('sqlite/addRedisServer', serverDetails);
      this.$emit('showModal', false);
    },
  },
};
</script>
