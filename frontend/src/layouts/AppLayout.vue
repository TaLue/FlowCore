<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 flex transition-colors">
    <!-- Sidebar -->
    <aside class="w-60 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col">
      <!-- Logo -->
      <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
            <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <span class="font-bold text-gray-900 dark:text-white">FlowCore</span>
        </div>
      </div>

      <!-- Nav -->
      <nav class="flex-1 px-3 py-4 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors"
          :class="$route.path === item.to
            ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400'
            : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white'"
        >
          <span class="text-base">{{ item.icon }}</span>
          {{ item.label }}
        </RouterLink>
      </nav>

      <!-- User + controls -->
      <div class="px-3 py-4 border-t border-gray-200 dark:border-gray-700 space-y-2">
        <!-- Dark mode toggle -->
        <button
          @click="theme.toggle()"
          class="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white transition-colors"
        >
          <svg v-if="theme.isDark" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364-6.364l-.707.707M6.343 17.657l-.707.707M17.657 17.657l-.707-.707M6.343 6.343l-.707-.707M12 7a5 5 0 100 10A5 5 0 0012 7z" />
          </svg>
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
          </svg>
          {{ theme.isDark ? 'โหมดสว่าง' : 'โหมดมืด' }}
        </button>

        <!-- User info + logout -->
        <div class="flex items-center gap-3 px-3 py-2">
          <div class="w-8 h-8 bg-blue-100 dark:bg-blue-900/50 rounded-full flex items-center justify-center text-blue-700 dark:text-blue-400 font-semibold text-sm">
            {{ auth.user?.username?.charAt(0).toUpperCase() }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ auth.user?.username }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ auth.user?.role }}</p>
          </div>
          <button @click="handleLogout" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200" title="ออกจากระบบ">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
          </button>
        </div>
      </div>
    </aside>

    <!-- Main -->
    <main class="flex-1 overflow-auto">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'

const router = useRouter()
const auth = useAuthStore()
const theme = useThemeStore()

const baseItems = [
  { to: '/', icon: '📊', label: 'Dashboard' },
  { to: '/requests', icon: '📋', label: 'คำขออนุมัติ' },
  { to: '/approvals', icon: '✅', label: 'รออนุมัติ' },
  { to: '/workflows', icon: '⚙️', label: 'Workflow' },
]

const adminItems = [
  { to: '/admin/users', icon: '👥', label: 'ผู้ใช้งาน' },
  { to: '/admin/departments', icon: '🏢', label: 'แผนก' },
]

const navItems = computed(() =>
  auth.isAdmin ? [...baseItems, ...adminItems] : baseItems
)

function handleLogout() {
  auth.logout()
  router.push('/login')
}
</script>
