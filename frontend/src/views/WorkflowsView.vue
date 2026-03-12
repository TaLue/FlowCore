<template>
  <div class="p-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-6">Workflow</h1>

    <div class="bg-white rounded-xl border border-gray-200">
      <div v-if="loading" class="p-8 text-center text-gray-400">กำลังโหลด...</div>
      <div v-else-if="!workflow" class="p-8 text-center text-gray-400">ไม่มีข้อมูล Workflow</div>
      <div v-else class="p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-base font-semibold text-gray-900">{{ workflow.name }}</h2>
          <span :class="workflow.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'"
            class="px-2 py-1 rounded-full text-xs font-medium">
            {{ workflow.isActive ? 'Active' : 'Inactive' }}
          </span>
        </div>
        <div class="space-y-2">
          <div v-for="(step, i) in workflow.steps" :key="i"
            class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg text-sm">
            <span class="w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-xs font-bold">
              {{ step.stepOrder }}
            </span>
            <span class="font-medium text-gray-900">{{ step.stepName }}</span>
            <span class="text-gray-500">— {{ step.approverType }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '@/api/client'

const workflow = ref<any>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await client.get('/workflows')
    const data = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
    workflow.value = data[0] ?? null
  } finally {
    loading.value = false
  }
})
</script>
