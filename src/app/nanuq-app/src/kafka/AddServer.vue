<template>
  <v-sheet class="mx-auto" width="300">
    <v-form fast-fail @submit.prevent>
      <v-text-field
        v-model="bootstrapServer"
        label="Bootstrap Server"
      ></v-text-field>

      <v-text-field
        v-model="alias"
        label="Alias"
      ></v-text-field>

      <v-btn class="mt-2" @click="saveServer" type="submit" block>Submit</v-btn>
      <br />
    </v-form>
  </v-sheet>
</template>
<script>

export default {
  name: 'AddServer',
  data() {
    return {
      bootstrapServer: '',
      alias: '',
    };
  },
  methods: {
    saveServer() {
      const serverDetails = { bootstrapServer: this.bootstrapServer, alias: this.alias };
      this.$store.dispatch('sqlite/addKafkaServer', serverDetails);
      this.$emit('showModal', false);
    },
  },
};
</script>
