<script setup>
</script>

<template>
    <div class="app-logo">
    </div>
    <router-view />

</template>

<style scoped>
.app-logo { position: fixed; top: 12px; left: 12px; z-index: 9999; }
.app-logo :deep(.logo-badge) { width: 48px; height: 48px; }
.app-logo :deep(.logo-badge) .logo-text { color: #000; }
.app-logo :deep(.logo-badge):hover { transform: translateY(-2px); box-shadow: 0 10px 26px rgba(0,0,0,0.22); }

</style>
