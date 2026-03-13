<template>
  <div class="p-8 max-w-3xl">
    <!-- Back -->
    <button @click="router.push('/requests')" class="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 mb-5">
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
      กลับรายการ
    </button>

    <div v-if="loading" class="text-center text-gray-400 py-16">กำลังโหลด...</div>
    <div v-else-if="!request" class="text-center text-gray-400 py-16">ไม่พบคำขอ</div>

    <template v-else>
      <!-- Header Card -->
      <div class="bg-white rounded-xl border border-gray-200 p-6 mb-5">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h1 class="text-xl font-bold text-gray-900 mb-1">{{ request.title }}</h1>
            <p class="text-sm text-gray-500">{{ request.requestTypeName }}</p>
          </div>
          <span :class="statusClass(request.status)" class="px-3 py-1 rounded-full text-xs font-semibold whitespace-nowrap">
            {{ statusLabel(request.status) }}
          </span>
        </div>

        <div class="grid grid-cols-2 gap-4 mt-5 pt-5 border-t border-gray-100 text-sm">
          <div>
            <p class="text-gray-400 text-xs mb-0.5">ผู้ขอ</p>
            <p class="font-medium text-gray-800">{{ request.requesterName }}</p>
          </div>
          <div>
            <p class="text-gray-400 text-xs mb-0.5">วันที่สร้าง</p>
            <p class="font-medium text-gray-800">{{ formatDate(request.createdAt) }}</p>
          </div>
          <div v-if="request.updatedAt !== request.createdAt">
            <p class="text-gray-400 text-xs mb-0.5">แก้ไขล่าสุด</p>
            <p class="font-medium text-gray-800">{{ formatDate(request.updatedAt) }}</p>
          </div>
          <div>
            <p class="text-gray-400 text-xs mb-0.5">ขั้นตอนปัจจุบัน</p>
            <p class="font-medium text-gray-800">{{ request.currentStep > 0 ? 'ขั้นตอนที่ ' + request.currentStep : '-' }}</p>
          </div>
        </div>

        <!-- Actions -->
        <div v-if="canSubmit" class="flex gap-3 mt-5 pt-5 border-t border-gray-100">
          <button @click="submitRequest" :disabled="acting"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
            {{ acting ? 'กำลังส่ง...' : 'ส่งคำขออนุมัติ' }}
          </button>
        </div>
        <p v-if="actionError" class="text-sm text-red-500 mt-3">{{ actionError }}</p>
      </div>

      <!-- Approval History -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <h2 class="text-sm font-semibold text-gray-700 mb-4">ประวัติการอนุมัติ</h2>
        <div v-if="request.approvals.length === 0" class="text-sm text-gray-400 py-4 text-center">
          ยังไม่มีประวัติการอนุมัติ
        </div>
        <div v-else class="space-y-3">
          <div v-for="a in sortedApprovals" :key="a.id"
            class="flex items-start gap-3 p-3 rounded-lg bg-gray-50">
            <div class="w-7 h-7 rounded-full bg-blue-600 text-white flex items-center justify-center text-xs font-bold shrink-0 mt-0.5">
              {{ a.stepOrder }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2">
                <span class="text-sm font-medium text-gray-800">{{ a.approverName }}</span>
                <span v-if="a.action" :class="actionClass(a.action)" class="px-2 py-0.5 rounded-full text-xs font-semibold">
                  {{ actionLabel(a.action) }}
                </span>
                <span v-else class="px-2 py-0.5 rounded-full text-xs font-semibold bg-yellow-100 text-yellow-700">
                  รอดำเนินการ
                </span>
              </div>
              <p v-if="a.comment" class="text-xs text-gray-500 mt-1">{{ a.comment }}</p>
              <p class="text-xs text-gray-400 mt-0.5">{{ formatDate(a.createdAt) }}</p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import client from '@/api/client'

const router = useRouter()
const route = useRoute()

const request = ref<any>(null)
const loading = ref(true)
const acting = ref(false)
const actionError = ref('')

onMounted(async () => {
  try {
    const res = await client.get('/requests/' + route.params.id)
    request.value = res.data
  } catch {
    request.value = null
  } finally {
    loading.value = false
  }
})

const canSubmit = computed(() => {
  const s = request.value?.status
  return s === 0 || s === 4
})

const sortedApprovals = computed(() =>
  [...(request.value?.approvals ?? [])].sort((a, b) => a.stepOrder - b.stepOrder)
)

async function submitRequest() {
  acting.value = true
  actionError.value = ''
  try {
    const res = await client.post('/requests/' + route.params.id + '/submit')
    request.value = res.data
  } catch (e: any) {
    actionError.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด'
  } finally {
    acting.value = false
  }
}

const STATUS_MAP: Record<number, { label: string; cls: string }> = {
  0: { label: 'ร่าง', cls: 'bg-gray-100 text-gray-600' },
  1: { label: 'รอดำเนินการ', cls: 'bg-blue-100 text-blue-700' },
  2: { label: 'อนุมัติแล้ว', cls: 'bg-green-100 text-green-700' },
  3: { label: 'ปฏิเสธ', cls: 'bg-red-100 text-red-700' },
  4: { label: 'ส่งกลับแก้ไข', cls: 'bg-orange-100 text-orange-700' },
}

function statusClass(s: string | number) {
  return STATUS_MAP[s]?.cls ?? 'bg-gray-100 text-gray-600'
}
function statusLabel(s: string | number) {
  return STATUS_MAP[s]?.label ?? String(s)
}

const ACTION_MAP: Record<string, { label: string; cls: string }> = {
  Approve: { label: 'อนุมัติ', cls: 'bg-green-100 text-green-700' },
  Reject: { label: 'ปฏิเสธ', cls: 'bg-red-100 text-red-700' },
  Return: { label: 'ส่งกลับ', cls: 'bg-orange-100 text-orange-700' },
}

function actionClass(a: string) {
  return ACTION_MAP[a]?.cls ?? 'bg-gray-100 text-gray-600'
}
function actionLabel(a: string) {
  return ACTION_MAP[a]?.label ?? a
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>
