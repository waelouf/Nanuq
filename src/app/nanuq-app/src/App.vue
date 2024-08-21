<template>
  <nav>
    <router-link to="/">Home</router-link> |
    <router-link to="/kafka">Kafka</router-link>

  </nav>
  <main>
    <router-view />
  </main>
</template>
<script>

export default {
  name: 'App',
};
</script>

<style>
body{
  background: linear-gradient(to bottom, #555, #999);
  background-attachment: fixed;
}
main{
  margin: 20px ;
  padding: 30px;
  background-color: white;

  min-height: 300px;
}
#app {
  font-family: "Raleway", "Trebuchet MS", sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

nav {
  padding: 30px;
}

nav a {
  font-weight: bold;
  color: #2c3e50;
}

nav a.router-link-exact-active {
  color: white;
}
</style>
