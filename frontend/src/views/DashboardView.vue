<template>
  <div class="p-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-2">Dashboard</h1>
    <p class="text-gray-500 mb-8">ยินดีต้อนรับ, {{ auth.user?.username }}</p>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <p class="text-sm text-gray-500">คำขอทั้งหมด</p>
        <p class="text-3xl font-bold text-gray-900 mt-1">{{ stats.total }}</p>
      </div>
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <p class="text-sm text-gray-500">รออนุมัติ</p>
        <p class="text-3xl font-bold text-yellow-600 mt-1">{{ stats.pending }}</p>
      </div>
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <p class="text-sm text-gray-500">อนุมัติแล้ว</p>
        <p class="text-3xl font-bold text-green-600 mt-1">{{ stats.approved }}</p>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="bg-white rounded-xl border border-gray-200 p-6">
      <h2 class="text-base font-semibold text-gray-900 mb-4">ดำเนินการด่วน</h2>
      <div class="flex gap-3">
        <RouterLink
          to="/requests"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors"
        >
          สร้างคำขอใหม่
        </RouterLink>
        <RouterLink
          to="/approvals"
          class="px-4 py-2 bg-white hover:bg-gray-50 text-gray-700 text-sm font-medium rounded-lg border border-gray-300 transition-colors"
        >
          ดูรายการรออนุมัติ
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import client from '@/api/client'

const auth = useAuthStore()
const stats = ref({ total: 0, pending: 0, approved: 0 })

onMounted(async () => {
  try {
    const [reqRes, pendingRes] = await Promise.all([
      client.get('/requests'),
      client.get('/approvals/pending'),
    ])
    const requests = Array.isArray(reqRes.data) ? reqRes.data : (reqRes.data?.value ?? [])
    stats.value.total = requests.length
    const pendingData = pendingRes.data
    stats.value.pending = Array.isArray(pendingData) ? pendingData.length : (pendingData?.value?.length ?? 0)
    stats.value.approved = requests.filter((r: any) => r.status === 'Approved').length
  } catch {
    // ignore
  }
})
</script>
