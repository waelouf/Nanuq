// src/plugins/vuetify.js
import { createVuetify } from 'vuetify';

// Import only the styles needed
import 'vuetify/styles';

// Import only the components used in the app
import {
  VAlert,
  VApp,
  VAppBar,
  VBtn,
  VCard,
  VCardActions,
  VCardText,
  VCardTitle,
  VChip,
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
  VSheet,
  VSnackbar,
  VSpacer,
  VTable,
  VTabs,
  VTab,
  VTabsWindow,
  VTabsWindowItem,
  VTextField,
  VTextarea,
  VToolbar,
  VToolbarTitle,
  VTooltip,
} from 'vuetify/components';

// Import only the directives used in the app
import { Ripple } from 'vuetify/directives';

// Create the Vuetify instance with only what's needed
export default createVuetify({
  components: {
    VAlert,
    VApp,
    VAppBar,
    VBtn,
    VCard,
    VCardActions,
    VCardText,
    VCardTitle,
    VChip,
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
    VSheet,
    VSnackbar,
    VSpacer,
    VTable,
    VTabs,
    VTab,
    VTabsWindow,
    VTabsWindowItem,
    VTextField,
    VTextarea,
    VToolbar,
    VToolbarTitle,
    VTooltip,
  },
  directives: {
    Ripple,
  },
  theme: {
    defaultTheme: 'light',
  },
});
