<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2" style="color: #0078D4;">mdi-microsoft-azure</v-icon>
        <span>Manage Azure Service Bus</span>
      </v-card-title>

      <v-tabs v-model="tab" color="primary">
        <v-tab value="queues">
          <v-icon start>mdi-message-processing</v-icon>
          Queues
        </v-tab>
        <v-tab value="topics">
          <v-icon start>mdi-bullhorn-outline</v-icon>
          Topics
        </v-tab>
      </v-tabs>

      <v-tabs-window v-model="tab" class="pa-4">
        <v-tabs-window-item value="queues">
          <ManageQueues :server-id="serverId" />
        </v-tabs-window-item>

        <v-tabs-window-item value="topics">
          <ManageTopics :server-id="serverId" />
        </v-tabs-window-item>
      </v-tabs-window>
    </v-card>
  </v-container>
</template>

<script>
import ManageQueues from './servicebus/ManageQueues.vue';
import ManageTopics from './servicebus/ManageTopics.vue';

export default {
  name: 'ManageAzure',
  components: {
    ManageQueues,
    ManageTopics,
  },
  props: {
    serverId: {
      type: [String, Number],
      required: true,
    },
  },
  data() {
    return {
      tab: 'queues',
    };
  },
};
</script>
