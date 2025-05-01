// src/plugins/vuetify.js
import { createVuetify } from 'vuetify';

// Import only the styles needed
import 'vuetify/styles';

// Import only the components used in the app
import {
  VApp,
  VAppBar,
  VBtn,
  VCard,
  VCardActions,
  VCardText,
  VCardTitle,
  VContainer,
  VDialog,
  VDivider,
  VForm,
  VIcon,
  VList,
  VListItem,
  VListItemTitle,
  VMain,
  VProgressCircular,
  VRow,
  VCol,
  VSelect,
  VSnackbar,
  VSpacer,
  VTable,
  VTabs,
  VTab,
  VTabsItems,
  VTabItem,
  VTextField,
  VToolbar,
  VToolbarTitle,
} from 'vuetify/components';

// Import only the directives used in the app
import { Ripple } from 'vuetify/directives';

// Create the Vuetify instance with only what's needed
export default createVuetify({
  components: {
    VApp,
    VAppBar,
    VBtn,
    VCard,
    VCardActions,
    VCardText,
    VCardTitle,
    VContainer,
    VDialog,
    VDivider,
    VForm,
    VIcon,
    VList,
    VListItem,
    VListItemTitle,
    VMain,
    VProgressCircular,
    VRow,
    VCol,
    VSelect,
    VSnackbar,
    VSpacer,
    VTable,
    VTabs,
    VTab,
    VTabsItems,
    VTabItem,
    VTextField,
    VToolbar,
    VToolbarTitle,
  },
  directives: {
    Ripple,
  },
  theme: {
    defaultTheme: 'light',
  },
});
