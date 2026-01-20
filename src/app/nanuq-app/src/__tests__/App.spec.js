import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import { createRouter, createWebHistory } from 'vue-router';
import { createStore } from 'vuex';
import App from '../App.vue';

describe('App Component', () => {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/', component: { template: '<div>Home</div>' } },
    ],
  });

  // Mock Vuex store
  const store = createStore({
    modules: {
      notifications: {
        namespaced: true,
        state: {
          snackbar: {
            show: false,
            message: '',
            color: 'success',
            timeout: 3000,
          },
        },
        actions: {
          hide: () => {},
        },
      },
    },
  });

  it('should render the app', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router, store],
        stubs: {
          'router-link': true,
          'router-view': true,
          'v-snackbar': true,
          'v-btn': true,
        },
      },
    });

    expect(wrapper.exists()).toBe(true);
  });

  it('should contain navigation bar', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router, store],
        stubs: {
          'router-link': true,
          'router-view': true,
          'v-snackbar': true,
          'v-btn': true,
        },
      },
    });

    expect(wrapper.find('nav.navbar').exists()).toBe(true);
  });

  it('should display Nanuq brand name', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router, store],
        stubs: {
          'router-link': true,
          'router-view': true,
          'v-snackbar': true,
          'v-btn': true,
        },
      },
    });

    expect(wrapper.text()).toContain('Nanuq');
  });
});
