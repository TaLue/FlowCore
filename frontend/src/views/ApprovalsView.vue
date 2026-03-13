<template>
  <div class="p-8">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6">รออนุมัติ</h1>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 mb-4">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ชื่อคำขอ</label>
          <input
            v-model="filterTitle"
            type="text"
            placeholder="ค้นหาหัวข้อ..."
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ประเภท</label>
          <select
            v-model="filterTypeName"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">ทั้งหมด</option>
            <option v-for="t in uniqueTypes" :key="t" :value="t">{{ t }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ผู้ขอ</label>
          <input
            v-model="filterRequester"
            type="text"
            placeholder="ค้นหาผู้ขอ..."
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ช่วงวันที่</label>
          <div class="flex gap-1 items-center">
            <input
              v-model="filterDateFrom"
              type="date"
              class="w-full px-2 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            />
            <span class="text-gray-400 dark:text-gray-500 text-xs shrink-0">ถึง</span>
            <input
              v-model="filterDateTo"
              type="date"
              class="w-full px-2 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
      </div>
      <div class="flex items-center justify-between mt-3">
        <span class="text-xs text-gray-400 dark:text-gray-500">
          แสดง {{ filteredApprovals.length }} / {{ approvals.length }} รายการ
        </span>
        <button
          v-if="hasActiveFilter"
          @click="clearFilters"
          class="text-xs text-blue-600 hover:text-blue-800 font-medium"
        >
          ล้างตัวกรอง
        </button>
      </div>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
      <div v-if="loading" class="p-8 text-center text-gray-400 dark:text-gray-500">กำลังโหลด...</div>
      <div v-else-if="filteredApprovals.length === 0" class="p-8 text-center text-gray-400 dark:text-gray-500">
        {{ approvals.length === 0 ? 'ไม่มีรายการรออนุมัติ' : 'ไม่พบรายการที่ตรงกับเงื่อนไข' }}
      </div>
      <table v-else class="w-full text-sm">
        <thead class="border-b border-gray-200 dark:border-gray-700">
          <tr class="text-left text-gray-500 dark:text-gray-400">
            <th class="px-6 py-3 font-medium">คำขอ</th>
            <th class="px-6 py-3 font-medium">ประเภท</th>
            <th class="px-6 py-3 font-medium">ผู้ขอ</th>
            <th class="px-6 py-3 font-medium">ขั้นตอน</th>
            <th class="px-6 py-3 font-medium">วันที่</th>
            <th class="px-6 py-3 font-medium">การดำเนินการ</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-gray-700">
          <tr v-for="item in filteredApprovals" :key="item.approvalId" class="hover:bg-gray-50 dark:hover:bg-gray-700">
            <td class="px-6 py-4 font-medium text-gray-900 dark:text-white">{{ item.requestTitle }}</td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ item.requestTypeName }}</td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ item.requesterName }}</td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ item.stepName }}</td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ formatDate(item.createdAt) }}</td>
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
import { ref, computed, onMounted } from 'vue'
import client from '@/api/client'

const approvals = ref<any[]>([])
const loading = ref(true)

// Filters
const filterTitle = ref('')
const filterTypeName = ref('')
const filterRequester = ref('')
const filterDateFrom = ref('')
const filterDateTo = ref('')

const hasActiveFilter = computed(
  () =>
    !!filterTitle.value ||
    !!filterTypeName.value ||
    !!filterRequester.value ||
    !!filterDateFrom.value ||
    !!filterDateTo.value,
)

const uniqueTypes = computed(() => {
  const names = approvals.value.map((a) => a.requestTypeName).filter(Boolean)
  return [...new Set(names)].sort()
})

const filteredApprovals = computed(() =>
  approvals.value.filter((item) => {
    if (filterTitle.value && !item.requestTitle.toLowerCase().includes(filterTitle.value.toLowerCase()))
      return false
    if (filterTypeName.value && item.requestTypeName !== filterTypeName.value)
      return false
    if (filterRequester.value && !item.requesterName.toLowerCase().includes(filterRequester.value.toLowerCase()))
      return false
    if (filterDateFrom.value) {
      const from = new Date(filterDateFrom.value)
      from.setHours(0, 0, 0, 0)
      if (new Date(item.createdAt) < from) return false
    }
    if (filterDateTo.value) {
      const to = new Date(filterDateTo.value)
      to.setHours(23, 59, 59, 999)
      if (new Date(item.createdAt) > to) return false
    }
    return true
  }),
)

function clearFilters() {
  filterTitle.value = ''
  filterTypeName.value = ''
  filterRequester.value = ''
  filterDateFrom.value = ''
  filterDateTo.value = ''
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('th-TH')
}

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
