<script setup>
// small, reusable circular logo that links to home
</script>

<template>
  <router-link to="/" class="logo-badge" aria-label="回首頁">
    <span class="logo-text">To-Do</span>
  </router-link>
</template>

<style scoped>
.logo-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #ffffff; /* white circle for contrast */
  text-decoration: none;
  color: #000000; /* requested black font */
  font-weight: 700;
  font-size: 0.8rem;
  box-shadow: 0 6px 18px rgba(0,0,0,0.16);
  cursor: pointer;
}
.logo-text { color: #000; }
</style>
