<template>
  <RouterView />
</template>

<script setup lang="ts">
import { useThemeStore } from '@/stores/theme'
// Initialize theme on app mount (applies dark class to <html> if needed)
useThemeStore()
</script>
