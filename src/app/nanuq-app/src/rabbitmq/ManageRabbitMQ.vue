<template>
  <div class="row">
    <span class="mt-4">Manage "{{ this.serverUrl }}"</span>
    <ol class="breadcrumb mb-4">
      <li class="breadcrumb-item active" />
    </ol>

    <v-tabs v-model="tab" class="mb-4">
      <v-tab value="exchanges">
        <v-icon start>mdi-swap-horizontal</v-icon>
        Exchanges
      </v-tab>
      <v-tab value="queues">
        <v-icon start>mdi-format-list-bulleted</v-icon>
        Queues
      </v-tab>
    </v-tabs>

    <v-tabs-window v-model="tab">
      <!-- Exchanges Tab -->
      <v-tabs-window-item value="exchanges">
        <div class="card mb-4">
          <div class="datatable-wrapper datatable-loading no-footer">
            <div class="datatable-container">
              <table id="exchanges-table" class="datatable-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Durable</th>
                    <th>Auto Delete</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(exchange, index) in availableExchanges" :key="index">
                    <td>{{ exchange.name }}</td>
                    <td>{{ exchange.type }}</td>
                    <td>{{ exchange.durable ? 'Yes' : 'No' }}</td>
                    <td>{{ exchange.autoDelete ? 'Yes' : 'No' }}</td>
                    <td>
                      <a @click="deleteExchange(exchange.name)" class="delete-icon">
                        <i class="fa-regular fa-trash-can" />
                      </a>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <v-btn class="mt-2" @click="showAddExchangeDialog = true" type="submit" block>Add Exchange</v-btn>
      </v-tabs-window-item>

      <!-- Queues Tab -->
      <v-tabs-window-item value="queues">
        <div class="card mb-4">
          <div class="datatable-wrapper datatable-loading no-footer">
            <div class="datatable-container">
              <table id="queues-table" class="datatable-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Messages</th>
                    <th>Consumers</th>
                    <th>Durable</th>
                    <th />
                    <th />
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(queue, index) in availableQueues" :key="index">
                    <td>{{ queue.name }}</td>
                    <td>{{ queue.messageCount }}</td>
                    <td>{{ queue.consumerCount }}</td>
                    <td>{{ queue.durable ? 'Yes' : 'No' }}</td>
                    <td>
                      <span @click="showQueueDetailsDialog(queue.name)" class="detail-link">
                        Details
                      </span>
                    </td>
                    <td>
                      <a @click="deleteQueue(queue.name)" class="delete-icon">
                        <i class="fa-regular fa-trash-can" />
                      </a>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <v-btn class="mt-2" @click="showAddQueueDialog = true" type="submit" block>Add Queue</v-btn>
      </v-tabs-window-item>
    </v-tabs-window>

    <!-- Dialogs -->
    <v-dialog v-model="showAddExchangeDialog" width="600px">
      <v-card prepend-icon="mdi-swap-horizontal">
        <AddExchange :serverUrl="serverUrl" @close="handleExchangeAdded" />
      </v-card>
    </v-dialog>

    <v-dialog v-model="showAddQueueDialog" width="600px">
      <v-card prepend-icon="mdi-format-list-bulleted">
        <AddQueue :serverUrl="serverUrl" @close="handleQueueAdded" />
      </v-card>
    </v-dialog>

    <v-dialog v-model="showQueueDetailsDialogFlag" width="600px">
      <v-card prepend-icon="mdi-information">
        <QueueDetails :serverUrl="serverUrl" :queueName="selectedQueueName" @close="showQueueDetailsDialogFlag = false" />
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
import AddExchange from './AddExchange.vue';
import AddQueue from './AddQueue.vue';
import QueueDetails from './QueueDetails.vue';

export default {
  name: 'ManageRabbitMQ',
  components: { AddExchange, AddQueue, QueueDetails },
  props: {
    serverUrl: {
      type: String,
      required: true,
    },
  },
  data() {
    return {
      tab: 'exchanges',
      showAddExchangeDialog: false,
      showAddQueueDialog: false,
      showQueueDetailsDialogFlag: false,
      selectedQueueName: '',
    };
  },
  async created() {
    await this.$store.dispatch('rabbitmq/loadExchanges', this.serverUrl);
    await this.$store.dispatch('rabbitmq/loadQueues', this.serverUrl);
  },
  computed: {
    availableExchanges() {
      return this.$store.state.rabbitmq.exchanges[this.serverUrl] || [];
    },
    availableQueues() {
      return this.$store.state.rabbitmq.queues[this.serverUrl] || [];
    },
  },
  methods: {
    async deleteExchange(name) {
      await this.$store.dispatch('rabbitmq/deleteExchange', {
        serverUrl: this.serverUrl,
        name,
      });
      this.reloadExchanges();
    },
    async deleteQueue(name) {
      await this.$store.dispatch('rabbitmq/deleteQueue', {
        serverUrl: this.serverUrl,
        name,
      });
      this.reloadQueues();
    },
    showQueueDetailsDialog(queueName) {
      this.selectedQueueName = queueName;
      this.showQueueDetailsDialogFlag = true;
    },
    handleExchangeAdded() {
      this.showAddExchangeDialog = false;
      this.reloadExchanges();
    },
    handleQueueAdded() {
      this.showAddQueueDialog = false;
      this.reloadQueues();
    },
    reloadExchanges() {
      this.$store.dispatch('rabbitmq/loadExchanges', this.serverUrl);
    },
    reloadQueues() {
      this.$store.dispatch('rabbitmq/loadQueues', this.serverUrl);
    },
  },
};
</script>

<style scoped>
.delete-icon {
  color: blue;
  cursor: pointer;
}
.detail-link {
  color: blue;
  cursor: pointer;
  text-decoration: underline;
}
</style>
