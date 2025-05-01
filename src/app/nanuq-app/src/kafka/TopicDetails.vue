<template>
  <div>
    <v-card :title="topicName">
      <v-card-text v-if="loading">
        Number of messages: {{ numberOfMessages }}
      </v-card-text>
      <v-card-text v-if="!loading">
        Loading...
      </v-card-text>
      <v-card-actions>
        <v-btn
          text="Close"
          class="ms-auto"
          @click="$emit('showModal', false)"
        />
      </v-card-actions>
    </v-card>
  </div>
</template>
<script>
import { ref, onMounted } from 'vue';
import { useStore } from 'vuex';

export default {
  name: 'TopicDetails',
  setup(props) {
    const numberOfMessages = ref(0);
    const store = useStore();
    const loading = ref(false);

    onMounted(async () => {
      try {
        const params = { serverName: props.serverName, topicName: props.topicName };
        const key = `${props.serverName}-${props.topicName}`;
        // Trigger the API call through store.dispatch
        if (store.state.kafka.kafkaTopicDetails[key] === undefined) {
          await store.dispatch('kafka/loadKafkaTopicDetails', params);
        }

        numberOfMessages.value = store.state.kafka.kafkaTopicDetails[key];
        loading.value = true;
      } catch (error) {
        import('@/utils/logger').then((module) => {
          const logger = module.default;
          logger.error('TopicDetails', 'Failed to load topic details', error);
        });
      } finally {
        //
      }
    });

    return {
      numberOfMessages,
      loading,
    };
  },
  props: {
    serverName: {
      type: String,
      required: true,
    },
    topicName: {
      type: String,
      required: true,
    },
  },
  methods: {
  },
};
</script>
<style>

</style>
