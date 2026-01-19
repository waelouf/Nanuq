<template>
  <head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <meta name="description" content="Nanuq" />
    <meta name="author" content="Wael Mansour" />
    <title>Nanuq</title>
  </head>
  <body class="sb-nav-fixed">
    <nav class="sb-topnav navbar navbar-expand navbar-dark bg-dark">
      <!-- Navbar Brand-->
      <router-link to="/" class="navbar-brand ps-3"><img src="./assets/logo.png" alt="Nanuq" class="logo-header">Nanuq</router-link>
      <!-- Sidebar Toggle-->
      <button type="button" class="btn btn-link btn-sm order-1 order-lg-0 me-4 me-lg-0" id="sidebarToggle" href="#"><i class="fas fa-bars" /></button>
      <!-- Navbar Search-->
      <form class="d-none d-md-inline-block form-inline ms-auto me-0 me-md-3 my-2 my-md-0">
        <div class="input-group" />
      </form>
      <!-- Navbar-->
      <ul class="navbar-nav ms-auto ms-md-0 me-3 me-lg-4">
        <li class="nav-item dropdown">
          <a
            class="nav-link dropdown-toggle"
            id="navbarDropdown"
            href="#"
            role="button"
            data-bs-toggle="dropdown"
            aria-expanded="false"><i class="fas fa-user fa-fw" /></a>
          <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="navbarDropdown">
            <li><a class="dropdown-item" href="#!">Settings</a></li>
            <li><a class="dropdown-item" href="#!" @click.prevent="$router.push('/activitylog')">Activity Log</a></li>
            <li><hr class="dropdown-divider" /></li>
            <li><a class="dropdown-item" href="#!">Logout</a></li>
          </ul>
        </li>
      </ul>
    </nav>
    <div id="layoutSidenav">
      <div id="layoutSidenav_nav">
        <nav class="sb-sidenav accordion sb-sidenav-dark" id="sidenavAccordion">
          <div class="sb-sidenav-menu">
            <div class="nav">
              <div class="sb-sidenav-menu-heading" />
              <router-link to="/" class="nav-link">
                <div class="sb-nav-link-icon"><i class="fas fa-tachometer-alt" /></div>
                Dashboard
              </router-link>
              <a class="nav-link collapsed" href="#" data-bs-toggle="collapse" data-bs-target="#collapseLayouts" aria-expanded="false" aria-controls="collapseLayouts">
                <div class="sb-nav-link-icon"><i class="fas fa-columns" /></div>
                Kafka
                <div class="sb-sidenav-collapse-arrow"><i class="fas fa-angle-down" /></div>
              </a>
              <div class="collapse" id="collapseLayouts" aria-labelledby="headingOne" data-bs-parent="#sidenavAccordion">
                <nav class="sb-sidenav-menu-nested nav">
                  <router-link to="/kafka/list" class="nav-link">List Servers</router-link>
                </nav>
              </div>
            </div>
            <div class="nav">
              <div class="sb-sidenav-menu-heading" />
              <a class="nav-link collapsed" href="#" data-bs-toggle="collapse" data-bs-target="#collapseLayoutsRedis" aria-expanded="false" aria-controls="collapseLayouts">
                <div class="sb-nav-link-icon"><i class="fas fa-columns" /></div>
                Redis
                <div class="sb-sidenav-collapse-arrow"><i class="fas fa-angle-down" /></div>
              </a>
              <div class="collapse" id="collapseLayoutsRedis" aria-labelledby="headingOne" data-bs-parent="#sidenavAccordion">
                <nav class="sb-sidenav-menu-nested nav">
                  <router-link to="/redis" class="nav-link">List Servers</router-link>
                </nav>
              </div>
            </div>
            <div class="nav">
              <div class="sb-sidenav-menu-heading" />
              <a class="nav-link collapsed" href="#" data-bs-toggle="collapse" data-bs-target="#collapseLayoutsRabbitMQ" aria-expanded="false" aria-controls="collapseLayouts">
                <div class="sb-nav-link-icon"><i class="fas fa-columns" /></div>
                RabbitMQ
                <div class="sb-sidenav-collapse-arrow"><i class="fas fa-angle-down" /></div>
              </a>
              <div class="collapse" id="collapseLayoutsRabbitMQ" aria-labelledby="headingOne" data-bs-parent="#sidenavAccordion">
                <nav class="sb-sidenav-menu-nested nav">
                  <router-link to="/rabbitmq" class="nav-link">List Servers</router-link>
                </nav>
              </div>
            </div>
            <div class="nav">
              <div class="sb-sidenav-menu-heading" />
              <a class="nav-link collapsed" href="#" data-bs-toggle="collapse" data-bs-target="#collapseLayoutsAWS" aria-expanded="false" aria-controls="collapseLayouts">
                <div class="sb-nav-link-icon"><i class="fab fa-aws" /></div>
                AWS (SQS/SNS)
                <div class="sb-sidenav-collapse-arrow"><i class="fas fa-angle-down" /></div>
              </a>
              <div class="collapse" id="collapseLayoutsAWS" aria-labelledby="headingOne" data-bs-parent="#sidenavAccordion">
                <nav class="sb-sidenav-menu-nested nav">
                  <router-link to="/aws" class="nav-link">List Servers</router-link>
                </nav>
              </div>
            </div>
          </div>
          <div class="sb-sidenav-footer" />
        </nav>
      </div>
      <div id="layoutSidenav_content">
        <main>
          <div class="container-fluid px-4">
            <router-view />
          </div>
        </main>
        <footer class="py-4 bg-light mt-auto">
          <div class="container-fluid px-4">
            <div class="d-flex align-items-center justify-content-between small">
              <div class="text-muted" />
              <div />
            </div>
          </div>
        </footer>
      </div>
    </div>

    <!-- Global Notification Snackbar -->
    <v-snackbar
      v-model="snackbar.show"
      :color="snackbar.color"
      :timeout="snackbar.timeout"
      location="top right"
      multi-line
    >
      {{ snackbar.message }}
      <template v-slot:actions>
        <v-btn
          variant="text"
          @click="hideNotification"
        >
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </body>
</template>
<script>

export default {
  name: 'App',
  computed: {
    snackbar() {
      return this.$store.state.notifications.snackbar;
    },
  },
  methods: {
    hideNotification() {
      this.$store.dispatch('notifications/hide');
    },
  },
};
</script>

<style>
.logo-header{
width:60px;
margin-top: 5px;
}
</style>
