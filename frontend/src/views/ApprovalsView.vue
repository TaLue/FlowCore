<template>
  <div class="p-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-6">รออนุมัติ</h1>

    <div class="bg-white rounded-xl border border-gray-200">
      <div v-if="loading" class="p-8 text-center text-gray-400">กำลังโหลด...</div>
      <div v-else-if="approvals.length === 0" class="p-8 text-center text-gray-400">
        ไม่มีรายการรออนุมัติ
      </div>
      <table v-else class="w-full text-sm">
        <thead class="border-b border-gray-200">
          <tr class="text-left text-gray-500">
            <th class="px-6 py-3 font-medium">คำขอ</th>
            <th class="px-6 py-3 font-medium">ผู้ขอ</th>
            <th class="px-6 py-3 font-medium">ขั้นตอน</th>
            <th class="px-6 py-3 font-medium">การดำเนินการ</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="item in approvals" :key="item.approvalId" class="hover:bg-gray-50">
            <td class="px-6 py-4 font-medium text-gray-900">{{ item.requestTitle }}</td>
            <td class="px-6 py-4 text-gray-500">{{ item.requesterName }}</td>
            <td class="px-6 py-4 text-gray-500">{{ item.stepName }}</td>
            <td class="px-6 py-4">
              <div class="flex gap-2">
                <button @click="approve(item.approvalId)"
                  class="px-3 py-1 bg-green-600 hover:bg-green-700 text-white text-xs font-medium rounded-lg">
                  อนุมัติ
                </button>
                <button @click="reject(item.approvalId)"
                  class="px-3 py-1 bg-red-600 hover:bg-red-700 text-white text-xs font-medium rounded-lg">
                  ปฏิเสธ
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '@/api/client'

const approvals = ref<any[]>([])
const loading = ref(true)

onMounted(loadApprovals)

async function loadApprovals() {
  loading.value = true
  try {
    const res = await client.get('/approvals/pending')
    approvals.value = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
  } finally {
    loading.value = false
  }
}

async function approve(id: number) {
  await client.post(`/approvals/${id}/approve`, { comment: '' })
  await loadApprovals()
}

async function reject(id: number) {
  await client.post(`/approvals/${id}/reject`, { comment: '' })
  await loadApprovals()
}
</script>
