import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import { createRouter, createWebHistory } from 'vue-router';
import App from '../App.vue';

describe('App Component', () => {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/', component: { template: '<div>Home</div>' } },
    ],
  });

  it('should render the app', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router],
        stubs: {
          'router-link': true,
          'router-view': true,
        },
      },
    });

    expect(wrapper.exists()).toBe(true);
  });

  it('should contain navigation bar', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router],
        stubs: {
          'router-link': true,
          'router-view': true,
        },
      },
    });

    expect(wrapper.find('nav.navbar').exists()).toBe(true);
  });

  it('should display Nanuq brand name', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router],
        stubs: {
          'router-link': true,
          'router-view': true,
        },
      },
    });

    expect(wrapper.text()).toContain('Nanuq');
  });
});
